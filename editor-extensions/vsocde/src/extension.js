import * as vscode from 'vscode';
import * as core from './core.js';
import * as tpParser from './parser.js';
import * as tpScanner from './scanner.js';

// TODO: need to make an state system that says if the current filename is already being processed or not

/** @param {import('tpScanner').Scanner} - TBD */
let scanner;

/**
 * @param {import('vscode').ExtensionContext} context - The VSCode extension context
 */
export async function activate(context) {
    scanner = new tpScanner.Scanner();

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
            // TODO: reset our scanner because the workspace changed
            const openDocuments = vscode.workspace.textDocuments;
            for (const document of openDocuments) {
                await processDocument(document, true);
            }
        })
    );

    context.subscriptions.push(
        vscode.workspace.onDidOpenTextDocument(document => {
            processDocument(document, false);
        })
    );

    context.subscriptions.push(
        vscode.workspace.onDidSaveTextDocument(document => {
            processDocument(document, false);
        })
    );

    // TODO: bring back auto task Id generation on pressing return. But dont rescan the TODOs.

    // TODO: Implement this
    // context.subscriptions.push(
    //     vscode.languages.registerCompletionItemProvider(
    //         [{ language: 'taskp', scheme: 'file' }, { language: 'taskp', scheme: 'untitled' }],
    //         {
    //             provideCompletionItems(document, position) {
    //                 const line = document.lineAt(position.line).text.substring(0, position.character);
    //                 if (!line.includes('@')) return [];

    //                 const parser = new tpParser.Parser();
    //                 return parser.users.map(user => {
    //                     const item = new vscode.CompletionItem(user, vscode.CompletionItemKind.User);
    //                     item.insertText = user;
    //                     return item;
    //                 });
    //             }
    //         },
    //         '@'
    //     )
    // );

    // Run parser on currently open .taskp files when extension activates
    const openDocuments = vscode.workspace.textDocuments;
    for (const document of openDocuments) {
        await processDocument(document, true);
    }
}

export function deactivate() {
    scanner = null;
}

/**
 * Parses and updates a `.taskp` document.
 *
 * @async
 * @param {import('vscode').TextDocument} document - The VSCode document to process.
 * @param {boolean} useScanner - If true, instructs the scanner to rescan the entire codebase.
 */
async function processDocument(document, useScanner) {
    if (!document.fileName.endsWith(core.FILE_EXTENSION)) return;

    const parser = new tpParser.Parser();
    parser.parse(document, useScanner);
    if (useScanner && parser.issuesLineNumber != -1) {
        let scanData = await scanner.scan(document.fileName, parser.settings, parser.issuesLineNumber);
        //let scanData = scanner.getScanData(document.fileName);
        parser.addScanData(scanData)
    }
    
    //const updatedText = generateUpdatedText(parser, document);
    await applyTextEdit(document, parser.textData);

    // TODO: Reimplement this
    //applyTodoDecorations(document, parser.todos);
}

/**
 * Replaces the entire content of a text document with new text.
 *
 * @param {import('vscode').TextDocument} document - The VSCode document to update.
 * @param {string} newText - The new text to replace the document content with.
 */
async function applyTextEdit(document, newText) {
    console.log("Applying newText:", newText);
    await vscode.window.showTextDocument(document, { preserveFocus: false });
    const edit = new vscode.WorkspaceEdit();
    const fullRange = new vscode.Range(
        document.positionAt(0),
        document.positionAt(document.getText().length)
    );
    edit.replace(document.uri, fullRange, newText);
    const success = await vscode.workspace.applyEdit(edit);
    if (!success) {
        console.log("Failed to apply edit for:", document.uri.fsPath);
        console.log("Document is dirty:", document.isDirty, "Closed:", document.isClosed);
    }
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