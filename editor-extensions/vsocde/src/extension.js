import * as vscode from 'vscode';

// ID are max 5 digit hex. This is shared between anything that needs an ID.
const MIN_ID = 0;
const MAX_ID = 1048575;

const SECTIONS = {
    NONE: 'NONE',
    TASKS: 'TASKS',
    TODOS: 'TODOS',
    USERS: 'USERS',
    SETTINGS: 'SETTINGS',
};

class TaskpieaParser {

    /** @type {string} Current section being parsed (NONE, TASKS, USERS, SETTINGS, TODO) */
    currentSection = SECTIONS.NONE;
    /** @type {{ name: string, id: string }[]} Array of tasks with name and id */
    tasks = [];
    /** @type {{ keyword: string, file: string, line: number, content: string }[]} Array of TODO items */
    todos = [];
    /** @type {string[]} Array of user names */
    users = [];
    /** @type {{ [key: string]: string }} Object mapping setting keys to values */
    settings = {};
    /** @type {Set<string>} Set of used task IDs to ensure uniqueness */
    usedIds = new Set();

    constructor() { }

    reset() {
        this.currentSection = SECTIONS.NONE;
        this.tasks = [];
        //this.todos = [];
        this.users = [];
        this.settings = {};
        this.usedIds = new Set();
    }

    generateId() {
        let id;
        do {
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
    * @returns {{ tasks: { name: string, id: string }[], users: string[], settings: { [key: string]: string }, todos: { keyword: string, file: string, line: number, content: string }[] }} Parsed data from .taskp file
    */
    parse(document) {
        this.reset();
        const lines = document.getText().split('\n');

        for (let i = 0; i < lines.length; i++) {
            let line = lines[i];
            line = line.replace(/\r/g, "");

            if (line.trim() === "") continue;

            // find matches for words between [], allowing leading/trailing whitespace
            // ^\s* = optional leading whitespace
            // \[(\w+)\] = section name between []
            // \s*$ = optional trailing whitespace
            const sectionMatch = line.match(/^\s*\[(\w+)\]\s*$/);
            if (sectionMatch && SECTIONS[sectionMatch[1]]) {
                this.currentSection = SECTIONS[sectionMatch[1]];
                continue;
            }

            if (this.currentSection === SECTIONS.NONE) {
                continue;
            } else if (this.currentSection === SECTIONS.TASKS && line.match(/^\s*- /)) {
                this.parseTask(line);
            } else if (this.currentSection === SECTIONS.SETTINGS && line.includes(':')) {
                this.parseSetting(line);
            } else if (this.currentSection === SECTIONS.USERS && line.match(/^\s*- /)) {
                this.parseUser(line);
            }
        }

        //return { tasks: this.tasks, users: this.users, settings: this.settings, todos: this.todos };
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
            const taskName = taskMatch[1];
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
    * @param {string} line - The setting line to parse from a .taskp file
    */
    parseSetting(line) {
        const [key, value] = line.split(':').map(s => s.trim());
        this.settings[key] = value;
    }

    /**
    * @param {string} line - The user line to parse from a .taskp file
    */
    parseUser(line) {
        this.users.push(line.replace(/^\s*- /, ''));
    }

    /**
     * Scan workspace for TODO keywords
     * @param {string[]} keywords - Keywords to search for
     * @param {string[]} excludePatterns - Glob patterns to exclude
     */
    async scanTodos(keywords, excludePatterns) {
        this.todos = [];
        const files = await vscode.workspace.findFiles('**/*', `{${excludePatterns.join(',')}}`);
        for (const file of files) {
            const document = await vscode.workspace.openTextDocument(file);
            const text = document.getText().split('\n');
            for (let i = 0; i < text.length; i++) {
                const line = text[i];
                for (const keyword of keywords) {
                    if (line.includes(keyword)) {
                        this.todos.push({
                            keyword,
                            file: file.fsPath,
                            line: i + 1,
                            content: line.trim()
                        });
                    }
                }
            }
        }
    }
}

/**
 * @param {{ tasks: { name: string, id: string }[], users: string[], settings: { [key: string]: string }, todos: { keyword: string, file: string, line: number, content: string }[] }} parsed - Parsed data from .taskp file
 * @param {import('vscode').TextDocument} document - The VSCode document to process
 * @returns {string} Updated text with task IDs and TODO section
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
    let taskIndex = 0;

    for (let line of lines) {
        line = line.replace(/\r/g, "");
        const sectionMatch = line.match(/^\s*\[(\w+)\]\s*$/);
        if (sectionMatch && SECTIONS[sectionMatch[1]]) {
            currentSection = SECTIONS[sectionMatch[1]];
            output.push(line);
            if (currentSection === SECTIONS.TODOS) {
                for (const todo of parsed.todos) {
                    output.push(`- ${todo.content}`);
                }
                output.push('\r');
            }
            continue;
        }

        if (currentSection === SECTIONS.TODOS) {
            continue; // todo section is always overwritten from memory, so just ignore all these lines
        }

        if (currentSection === SECTIONS.TASKS && line.match(/^\s*- /)) {
            const taskMatch = line.match(/^\s*- (.+?)(?: \[#([A-Z0-9]{5})\])?\s*$/);
            if (!taskMatch || taskIndex >= parsed.tasks.length) {
                // Invalid task format, preserve original and go next
                // TODO: maybe in v2 we can add a "red squiggle" to say error or something
                output.push(line);
                continue;
            }

            const task = parsed.tasks[taskIndex++];
            if (!taskMatch[2]) {
                // add missing task ID
                output.push(`${line} [#${task.id}]`);
            } else {
                // replace existing ID with new one (handles duplicates or keeps same if unique)
                output.push(line.replace(/\[#[A-Z0-9]{5}\]/, `[#${task.id}]`));
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
export async function activate(context) {
    async function processDocument(document) {
        if (!document.fileName.endsWith('.taskp')) return;

        const parser = new TaskpieaParser();
        parser.parse(document);
        const keywords = parser.settings['todoKeywords']?.split(',').map(k => k.trim()) || ['TODO', 'FIXME'];
        const excludePatterns = parser.settings['excludePatterns']?.split(',').map(p => p.trim()) || ['**/*.taskp'];

        await parser.scanTodos(keywords, excludePatterns);
        const updatedText = generateUpdatedText(parser, document);
        applyTextEdit(document, updatedText);
    }

    context.subscriptions.push(
        vscode.workspace.onDidChangeWorkspaceFolders(async () => {
            const openDocuments = vscode.workspace.textDocuments;
            for (const document of openDocuments) {
                await processDocument(document);
            }
        })
    );

    context.subscriptions.push(
        vscode.workspace.onDidOpenTextDocument(processDocument)
    );

    context.subscriptions.push(
        vscode.workspace.onDidSaveTextDocument(processDocument)
    );

    // TODO: bring back auto task Id generation on pressing return. But dont rescan the TODOs.

    context.subscriptions.push(
        vscode.languages.registerCompletionItemProvider(
            [{ language: 'taskp', scheme: 'file' }, { language: 'taskp', scheme: 'untitled' }],
            {
                provideCompletionItems(document, position) {
                    const line = document.lineAt(position.line).text.substring(0, position.character);
                    if (!line.includes('@')) return [];

                    const parser = new TaskpieaParser();
                    return parser.users.map(user => {
                        const item = new vscode.CompletionItem(user, vscode.CompletionItemKind.User);
                        item.insertText = user;
                        return item;
                    });
                }
            },
            '@'
        )
    );

    // Run parser on currently open .taskp files when extension activates
    const openDocuments = vscode.workspace.textDocuments;
    for (const document of openDocuments) {
        await processDocument(document);
    }
}

export function deactivate() { }