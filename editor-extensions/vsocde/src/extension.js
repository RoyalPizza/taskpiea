import * as vscode from 'vscode';
import * as core from './core.js';
import * as tpParser from './parser.js';
import * as tpScanner from './scanner.js';

// TODO: need to make there are no edcases where process document is called async on the same file, or spammed.
//       perhaps just make a system that states a "process operation" is underway and ignore future requests?
//       or just que them.

/** 
 * Caches vscode.CompletionItem arrays for each Taskp file.
 * @type {Map<string, vscode.CompletionItem[]>}
 */
let userCompletionItems = new Map();

/**
 * @param {import('vscode').ExtensionContext} context - The VSCode extension context
 */
export async function activate(context) {
    console.log("activate");

    // TODO: review this
    // context.subscriptions.push(
    //     vscode.commands.registerCommand('taskpiea.jumpToTodo', async (args) => {
    //         const [file, line] = Array.isArray(args) ? args : JSON.parse(args);
    //         const document = await vscode.workspace.openTextDocument(file);
    //         await vscode.window.showTextDocument(document, {
    //             selection: new vscode.Range(line - 1, 0, line - 1, 0)
    //         });
    //     })
    // );

    context.subscriptions.push(
        vscode.workspace.onDidChangeWorkspaceFolders(async () => {
            // TODO: reset our scanner because the workspace changed?
            const openDocuments = vscode.workspace.textDocuments;
            for (const document of openDocuments) {
                //console.log("onDidChangeWorkspaceFolders " + document.fileName);
                if (!document.fileName.endsWith(core.FILE_EXTENSION)) return;
                await _processDocument(document, true);
            }
        })
    );

    context.subscriptions.push(
        vscode.workspace.onDidOpenTextDocument(document => {
            // this event fires when our code runs the scanner
            //console.log("onDidOpenTextDocument + " + document.fileName);
            if (!document.fileName.endsWith(core.FILE_EXTENSION)) return;
            _processDocument(document, false);
        })
    );

    context.subscriptions.push(
        vscode.workspace.onDidSaveTextDocument(document => {
            //console.log("onDidSaveTextDocument + " + document.fileName);
            if (!document.fileName.endsWith(core.FILE_EXTENSION)) return;
            _processDocument(document, false);
        })
    );

    // TODO: bring back auto task Id generation on pressing return. But dont rescan the TODOs.
    context.subscriptions.push(
        vscode.workspace.onDidChangeTextDocument(event => {
            if (!event.document.fileName.endsWith('.taskp')) return;
            _processDocumentChanges(event.document, event.contentChanges);
        })
    );

    context.subscriptions.push(
        vscode.languages.registerCompletionItemProvider(
            [{ language: 'taskp', scheme: 'file' }, { language: 'taskp', scheme: 'untitled' }],
            { provideCompletionItems: _provideCompletionItems.bind(this) },
            '@'
        )
    );

    // Run parser on currently open .taskp files when extension activates
    const openDocuments = vscode.workspace.textDocuments;
    for (const document of openDocuments) {
        await _processDocument(document, true);
    }
}

export function deactivate() {
    console.log("deactivate");
}

/**
 * Parses and updates a `.taskp` document.
 *
 * @async
 * @param {import('vscode').TextDocument} document - The VSCode document to process.
 * @param {boolean} useScanner - If true, instructs the scanner to rescan the entire codebase.
 */
async function _processDocument(document, useScanner) {
    if (!document.fileName.endsWith(core.FILE_EXTENSION)) return;
    //console.log("processing " + document.fileName);

    const parser = new tpParser.Parser();
    parser.parse(document, useScanner);
    if (useScanner && parser.issuesLineNumber != -1) {
        let scanner = new tpScanner.Scanner();
        let scanData = await scanner.scan(document.fileName, parser.settings, parser.issuesLineNumber);
        parser.addScanData(scanData)
    }

    const text = parser.textData.join('\n');
    await _applyTextEdit(document, text);

    // TODO: Reimplement this
    //applyTodoDecorations(document, parser.todos);
    _cacheUsersForAutocomplete(document.fileName, parser.users);
}

/**
 * Replaces the entire content of a VSCode text document with new text.
 *
 * This function works on both open and closed documents. It creates a full-range
 * WorkspaceEdit and applies it. The function validates the range before applying
 * the edit and logs warnings if the document is closed or if the edit fails.
 *
 * @param {import('vscode').TextDocument} document - The VSCode document to update.
 * @param {string} newText - The new text to replace the document content with.
 * @returns {Promise<boolean>} Resolves to true if the edit was successfully applied, false otherwise.
 */
async function _applyTextEdit(document, newText) {
    if (document.isClosed) {
        console.warn("Document is closed");
        return false;
    }

    const fullRange = new vscode.Range(
        document.positionAt(0),
        document.positionAt(document.getText().length) // TODO: pull this from a cache instead of calling this again
    );
    if (!document.validateRange(fullRange)) {
        console.warn(`Invalid edit range in ${document.uri.toString()}`);
        return false;
    }

    const edit = new vscode.WorkspaceEdit();
    edit.replace(document.uri, fullRange, newText);

    // TODO: this method will not mark the file as dirty of the taskp file is open. But hitting save will save changes. Find solution that shows the file as dirty.
    const success = await vscode.workspace.applyEdit(edit);
    if (!success) console.warn("Failed to apply edit for:", document.uri.fsPath);
    return success;
}

/**
 * @param {import('vscode').TextDocument} document - The VSCode document to decorate
 * @param {{ keyword: string, file: string, line: number, content: string, range?: vscode.Range }[]} todos - Array of TODO items
 */
function applyTodoDecorations(document, todos) {
    const decorationType = vscode.window.createTextEditorDecorationType({
        textDecoration: 'underline',
        cursor: 'pointer'
    });

    const decorations = todos
        .filter(todo => todo.range)
        .map(todo => ({
            range: todo.range,
            hoverMessage: new vscode.MarkdownString(`[Jump to ${todo.file}:${todo.line}](command:taskpiea.jumpToTodo?${encodeURIComponent(JSON.stringify([todo.file, todo.line]))})`)
        }));

    const editors = vscode.window.visibleTextEditors.filter(editor => editor.document === document);
    editors.forEach(editor => editor.setDecorations(decorationType, decorations));
}

/**
 * Caches vscode.CompletionItem objects for a specific Taskp file.
 *
 * Each string in the `users` array is converted into a CompletionItem
 * of kind `User` and stored in `this.userCompletionItems` keyed by filename.
 *
 * @param {string} filename - The name or path of the Taskp file.
 * @param {string[]} users - Array of user names to create completion items for.
 */
function _cacheUsersForAutocomplete(filename, users) {
    let completionItems = [];
    for (const user of users) {
        const item = new vscode.CompletionItem(user, vscode.CompletionItemKind.User);
        item.insertText = user;
        completionItems.push(item);
    }

    userCompletionItems.set(filename, completionItems);
}

/**
 * Provides completion items for user mentions in a Taskp document.
 * 
 * This function is called by the VSCode completion provider when the user types.
 * It only triggers if the current line contains an '@' character.
 * Completion items are retrieved from the cached user list for the current file.
 *
 * @param {import('vscode').TextDocument} document - The document in which completion is requested.
 * @param {import('vscode').Position} position - The position of the cursor in the document.
 * @returns {import('vscode').CompletionItem[]} An array of completion items for the current document.
 */
function _provideCompletionItems(document, position) {
    const line = document.lineAt(position.line).text.substring(0, position.character);
    if (!line.includes('@')) return [];
    //console.log("registerCompletionItemProvider");
    const items = userCompletionItems.get(document.fileName) || [];
    return [...items];
}

function _processDocumentChanges(document, changes) {
    if (changes.length === 0) return;
    //console.log("onDidChangeTextDocument + " + document.fileName);
    for (const change of changes) {
        if (change.text.includes('\n')) {
            _processDocument(document, false);
            return; // no need to check further, just perform the process
        }
    }
}