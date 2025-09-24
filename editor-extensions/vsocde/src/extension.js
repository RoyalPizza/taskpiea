import * as vscode from 'vscode';

// ID are max 5 digit hex. This is shared between anything that needs an ID.
const MIN_ID = 0;
const MAX_ID = 1048575;

const SECTIONS = {
    NONE: 'NONE',
    TASKS: 'TASKS',
    USERS: 'USERS',
    SETTINGS: 'SETTINGS'
};

class TaskpieaParser {
    constructor() {
        // TODO: make these as lets at the top of the class with comments so their type is known.
        // users is just a string
        // tasks is "taskName" and "taskId"
        // settings is just a string
        this.currentSection = SECTIONS.NONE;
        this.tasks = [];
        this.users = [];
        this.settings = {};
        this.usedIds = new Set();
    }

    reset() {
        this.currentSection = SECTIONS.NONE;
        this.tasks = [];
        this.users = [];
        this.settings = {};
        this.usedIds = new Set();
    }

    generateId() {
        let id;
        do {
            // Generate random 5-char hex ID (0-FFFFF)
            id = Math.floor(Math.random() * (MAX_ID + 1))
                .toString(16)
                .padStart(5, '0')
                .toUpperCase();
        } while (this.usedIds.has(id));
        this.usedIds.add(id);
        return id;
    }

    /**
    * @param {import('vscode').TextDocument} document - The VSCode document to parse
    * @returns {{ tasks: { name: string, id: string }[], users: string[], settings: { [key: string]: string } }} Parsed data from .taskp file
    */
    parse(document) {
        this.reset(); // internal cache is ALWAYS rebuilt
        const lines = document.getText().split('\n');

        for (let i = 0; i < lines.length; i++) {
            let line = lines[i];
            line = line.replace(/\r/g, "");

            if (line.trim() === "") continue; // empty line or whitespace only, go next

            // find matches for words between [], allowing leading/trailing whitespace
            // ^\s* = optional leading whitespace
            // \[(\w+)\] = section name between []
            // \s*$ = optional trailing whitespace
            const sectionMatch = line.match(/^\s*\[(\w+)\]\s*$/);
            if (sectionMatch && SECTIONS[sectionMatch[1]]) {
                this.currentSection = SECTIONS[sectionMatch[1]];
                continue; // this was a section line, go next
            }

            if (this.currentSection === SECTIONS.NONE) {
                continue; // not in a section, go next
            } else if (this.currentSection === SECTIONS.TASKS && line.match(/^\s*- /)) {
                this.parseTask(line);
            } else if (this.currentSection === SECTIONS.SETTINGS && line.includes(':')) {
                this.parseSetting(line);
            } else if (this.currentSection === SECTIONS.USERS && line.match(/^\s*- /)) {
                this.parseUser(line);
            }
        }

        return { tasks: this.tasks, users: this.users, settings: this.settings };
    }

    /**
    * @param {string} line - The task line to parse from a .taskp file
    */
    parseTask(line) {
        // find matches in task line starting with "- ", capturing task name and optional task ID
        // ^\s* = optional leading whitespace
        // - = literal bullet point
        // (.+?) = task name (non-greedy)
        // (?: \[#([A-Z0-9]{5})\])? = optional task ID in [#XXXXX] format, 5 alphanumeric chars
        // \s*$ = optional trailing whitespace
        const taskMatch = line.match(/^\s*- (.+?)(?: \[#([A-Z0-9]{5})\])?\s*$/);
        if (taskMatch) {
            const taskName = taskMatch[1]; // keep untrimmed for consistency
            let taskId = taskMatch[2];

            // If task has an ID and it's already used, generate a new one
            if (taskId && this.usedIds.has(taskId)) {
                taskId = this.generateId();
            } else if (!taskId) {
                // If no ID, generate one
                taskId = this.generateId();
            } else {
                // If ID is unique, add it to usedIds
                this.usedIds.add(taskId);
            }

            this.tasks.push({ name: taskName, id: taskId });
        }
    }

    /**
    * @param {string} line - The task line to parse from a .taskp file
    */
    parseSetting(line) {
        // TODO: Ignore this for now, will test later
        return;
        const [key, value] = line.split(':').map(s => s.trim());
        this.settings[key] = value;
        if (key === 'lastId') {
            const idNum = parseInt(value, 16);
            if (idNum > this.lastId) this.lastId = idNum;
        }
    }

    /**
    * @param {string} line - The task line to parse from a .taskp file
    */
    parseUser(line) {
        // remove leading "- " and keep untrimmed content
        this.users.push(line.replace(/^\s*- /, ''));
    }
}

/**
 * @param {{ tasks: { name: string, id: string }[], users: string[], settings: { [key: string]: string } }} parsed - Parsed data from .taskp file
 * @param {import('vscode').TextDocument} document - The VSCode document to process
 * @returns {string} Updated text with task IDs added where missing
 */
function generateUpdatedText(parsed, document) {
    // TODO: consider if its worth double parsing. We parse once to put a bunch of objects in memory
    // then again to update the doc. Essentially looping through the whole document twice. For now, leave
    // as is. But after adding other features, see if we can limit this to one. Not sure I can do that yet 
    // until I add the other features.
    // TODO: decide why this should be seperate from parser. Right now it makes no sense. it should just return the text.
    // in the future that may not be true though, it just depends how "VSCode" integration works for things like
    // red squiggles, autocomplete, etc..

    const lines = document.getText().split('\n');
    let output = [];
    let currentSection = SECTIONS.NONE;
    let taskIndex = 0; // a gimmicky way to quick index into the tasks array without doing a lookup

    for (let line of lines) {
        line = line.replace(/\r/g, "");

        // determine section
        const sectionMatch = line.match(/^\s*\[(\w+)\]\s*$/);
        if (sectionMatch && SECTIONS[sectionMatch[1]]) {
            currentSection = SECTIONS[sectionMatch[1]];
            output.push(line);
            continue;
        }

        // process tasks
        if (currentSection === SECTIONS.TASKS && line.match(/^\s*- /)) {
            const taskMatch = line.match(/^\s*- (.+?)(?: \[#([A-Z0-9]{5})\])?\s*$/);
            if (!taskMatch || taskIndex >= parsed.tasks.length) {
                // Invalid task format, preserve original and go next
                // TODO: maybe in v2 we can add a "red squiggle" to say error or something
                output.push(line);
                continue;
            }

            // Only add ID to task if missing, otherwise preserve original
            const task = parsed.tasks[taskIndex++];
            if (!taskMatch[2]) {
                output.push(`${line} [#${task.id}]`);
            } else {
                output.push(line);
            }
        } else {
            // no need to process, go next
            output.push(line);
        }
    }

    return output.join('\n');
}

function applyTextEdit(document, newText) {
    const edit = new vscode.WorkspaceEdit();
    const fullRange = new vscode.Range(
        document.positionAt(0),
        document.positionAt(document.getText().length)
    );
    edit.replace(document.uri, fullRange, newText);
    vscode.workspace.applyEdit(edit);
}

/**
 * @param {import('vscode').ExtensionContext} context - The VSCode extension context
 */
export function activate(context) {
    const parser = new TaskpieaParser();

    context.subscriptions.push(
        vscode.workspace.onDidChangeWorkspaceFolders(() => {
            const openDocuments = vscode.workspace.textDocuments;
            for (const document of openDocuments) {
                if (document.fileName.endsWith('.taskp')) {
                    const parsed = parser.parse(document);
                    const updatedText = generateUpdatedText(parsed, document);
                    applyTextEdit(document, updatedText);
                }
            }
        })
    );

    context.subscriptions.push(
        vscode.workspace.onDidOpenTextDocument(document => {
            if (!document.fileName.endsWith('.taskp')) return;

            const parsed = parser.parse(document);
            const updatedText = generateUpdatedText(parsed, document);
            applyTextEdit(document, updatedText);
        })
    );

    context.subscriptions.push(
        vscode.workspace.onDidChangeTextDocument(event => {
            if (!event.document.fileName.endsWith('.taskp')) return;

            const document = event.document;
            const changes = event.contentChanges;
            if (changes.length === 0) return;

            // TODO: decide if I need to loop through changes or not
            const change = changes[0];
            const newText = change.text;

            if (newText.includes('\n')) {
                const parsed = parser.parse(document);
                const updatedText = generateUpdatedText(parsed, document);
                applyTextEdit(document, updatedText);
            }
        })
    );

    context.subscriptions.push(
        vscode.languages.registerCompletionItemProvider(
            [{ language: 'taskp', scheme: 'file' }, { language: 'taskp', scheme: 'untitled' }],
            {
                provideCompletionItems(document, position) {
                    const line = document.lineAt(position.line).text.substring(0, position.character);
                    if (!line.includes('@')) return [];

                    const parsed = parser.parse(document);
                    return parsed.users.map(user => {
                        const item = new vscode.CompletionItem(user, vscode.CompletionItemKind.User);
                        item.insertText = user;
                        return item;
                    });
                }
            },
            '@'
        )
    );
}

export function deactivate() { }