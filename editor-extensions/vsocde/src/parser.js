/**
 * parser.js
 *
 * Parses `.taskp` files into structured data including tasks, users, and settings.
 * Use the scanner.js to scan codebases and insert them as issues here.
 */

import * as vscode from 'vscode';
import * as core from './core.js';

// TODO: consider storing usedIds as ints for faster lookup over the hex string? (future)

export class Parser {

    constructor() {
        /** @type {{ name: string, id: string }[]} Array of tasks with name and id */
        this.tasks = [];

        /** @type {string[]} Array of user names */
        this.users = [];

        /** @type {{ key: string, value: string }[]} Array of settings allowing duplicate keys */
        this.settings = [];

        /** @type {Set<string>} Set of used task IDs to ensure uniqueness */
        this.usedIds = new Set();

        /** @type {string[]} An array representing each line of the text document. */
        this.textData = [];

        /** @type {int} The index into textData where [ISSUES] is. -1 means no section. */
        this.issuesLineNumber = -1;

        /** @type {DecorationOptions[]} An array of issue decorators to apply to the document. */
        this.issueDecorationOptions = []
    }

    /**
     * Generates a unique 5-digit hexadecimal ID string.
     * Ensures no collisions by checking against the current set of used IDs.
     *
     * @returns {string} A unique 5-digit uppercase hex ID (e.g., "0A3F2").
     */
    _generateId() {
        let id;
        do {
            id = Math.floor(Math.random() * (core.MAX_ID + 1))
                .toString(16)
                .padStart(5, '0')
                .toUpperCase();
        } while (this.usedIds.has(id));
        this.usedIds.add(id);
        return id;
    }

    /**
    * @param {import('vscode').TextDocument} document - The VSCode document to parse
    * @param {bool} - states if we are using the scanner or not. If true, ignore ISSUES section because it will be readded. If false, keep files issues section.
    * @returns {{ tasks: { name: string, id: string }[], users: string[], settings: { key: string, value: string } }} Parsed data from .taskp file
    */
    parse(document, useScanner) {
        let currentSection = core.SECTIONS.NONE;
        const lines = document.getText().split(/\r?\n/);

        for (let i = 0; i < lines.length; i++) {
            let line = lines[i];

            // find matches for words between [], allowing leading/trailing whitespace
            // ^\s* = optional leading whitespace
            // \[(\w+)\] = section name between []
            // \s*$ = optional trailing whitespace
            const sectionMatch = line.match(/^\s*\[(\w+)\]\s*$/);
            if (sectionMatch && core.SECTIONS[sectionMatch[1]]) {
                currentSection = core.SECTIONS[sectionMatch[1]];
                this.textData.push(line);
                if (currentSection === core.SECTIONS.ISSUES) this.issuesLineNumber = i;
                continue;
            }

            switch (currentSection) {
                case core.SECTIONS.NONE:
                    break;
                case core.SECTIONS.TASKS:
                    this._parseTask(line);
                    // textData handled inside _parseTask
                    break;
                case core.SECTIONS.ISSUES:
                    if (!useScanner) {
                        this._parseIssue(line, i);
                        this.textData.push(line);
                    } else {
                        this.textData.push('');
                    }
                    break;
                case core.SECTIONS.USERS:
                    this._parseUser(line);
                    this.textData.push(line);
                    break;
                case core.SECTIONS.SETTINGS:
                    this._parseSetting(line);
                    this.textData.push(line);
                    break;
                default:
                    break
            }
        }

        return { tasks: this.tasks, users: this.users, settings: this.settings };
    }

    /**
    * @param {string} line - The task line to parse from a .taskp file
    */
    _parseTask(line) {
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

            if (taskId && this.usedIds.has(taskId)) {
                // duplicate ID, generate new one
                taskId = this._generateId();
                this.textData.push(line.replace(/\[#[A-Z0-9]{5}\]/, `[#${taskId}]`));
            } else if (!taskId) {
                // no ID, generate one
                taskId = this._generateId();
                this.textData.push(`${line} [#${taskId}]`);
            } else {
                this.usedIds.add(taskId);
                this.textData.push(line);
            }

            this.tasks.push({ name: taskName, id: taskId });
        } else {
            this.textData.push(line);
        }
    }

    /**
     * Attempts to parse an inline issue reference from a line of text.
     * Only used when the scanner is disabled. If a match is found, an
     * issue decorator is created for the given line.
     *
     * @param {string} line - The line of text to check for an issue reference.
     * @param {number} lineNumber - The line number in the document.
     */
    _parseIssue(line, lineNumber) {
        // This is only called if we are not using the scanner. 
        // This will create decorators based on the line text.

        const issueMatch = line.match(/\[([^\[\]:]+)::(\d+)\]/);
        if (issueMatch) {
            this._createIssueDecorator(line, lineNumber, issueMatch[1], issueMatch[2])
        }
    }

    /**
    * @param {string} line - The user line to parse from a .taskp file
    */
    _parseUser(line) {
        if (!line.match(/^\s*- /))
            return;

        this.users.push(line.replace(/^\s*- /, '').trim());
    }

    /**
    * @param {string} line - The setting line to parse from a .taskp file
    */
    _parseSetting(line) {
        if (!line.includes(':'))
            return;

        const [key, value] = line.split(':').map(s => s.trim());
        this.settings.push({ key: key, value: value });
    }

    /**
     * Inserts scanned issues into the text data at the designated issues line.
     * @param {{ issues: { keyword: string, file: string, line: number, content: string, range?: vscode.Range }[] }} scanData - Object containing an array of issues returned from `scan`.
     */
    addScanData(scanData) {
        if (this.issuesLineNumber === -1 || !scanData?.issues) return;
        const issues = scanData.issues.map(issue => `- ${issue.content} [${issue.file}::${issue.lineNumber}]`);
        this.textData.splice(this.issuesLineNumber + 1, 0, ...issues);

        // create issue decorators from the issues. apply them with applyIssueDecorators
        for (let i = 0; i < scanData.issues.length; i++) {
            const issue = scanData.issues[i];
            const fullLine = `- ${issue.content} [${issue.file}::${issue.lineNumber}]`; // this is duplicate code
            this._createIssueDecorator(fullLine, this.issuesLineNumber + 1 + i, issue.file, issue.lineNumber);
        }
    }

    /**
     * Creates a single issue decoration for a line in a document.
     * The decoration is not applied immediately; it is stored in `this.issueDecorationOptions`
     * and can be applied later in bulk using `applyIssueDecorators`.
     *
     * @param {string} documentLine - The text content of the line to decorate.
     * @param {number} documentLineNumber - The 0-based line number in the document.
     * @param {string} issueFile - The filename of the issue target.
     * @param {number} issueLineNumber - The line number in the target file.
     */
    _createIssueDecorator(documentLine, documentLineNumber, issueFile, issueLineNumber) {
        const range = new vscode.Range(documentLineNumber, 0, documentLineNumber, documentLine.length);
        const uriComponent = encodeURIComponent(JSON.stringify([issueFile, issueLineNumber]));
        const hoverMessage = new vscode.MarkdownString(`[Jump to ${issueFile}:${issueLineNumber}](${core.COMMAND_JUMP_TO_ISSUE}?${uriComponent})`);
        this.issueDecorationOptions.push({ range, hoverMessage });
    }

    /**
     * Applies all stored issue decorations to a document.
     * Creates a decoration type for clickable underlines and applies
     * the accumulated `issueDecorationOptions` to all visible editors
     * displaying the document.
     *
     * @param {import('vscode').TextDocument} document - The VSCode document to decorate.
     */
    applyIssueDecorators(document) {

        const editors = vscode.window.visibleTextEditors.filter(editor => editor.document === document);
        editors.forEach(editor => editor.setDecorations(core.ISSUE_DECORATION_TYPE, this.issueDecorationOptions));
    }
}