import * as vscode from 'vscode';
import * as core from './core.js';

// TODO: this is a concept class. Either refactor it or remove the feature.

export class TaskpCodeLensProvider {

    /**
     * Provide CodeLenses for a document.
     * @param {vscode.TextDocument} document 
     * @param {vscode.CancellationToken} token 
     * @returns {vscode.CodeLens[]}
     */
    provideCodeLenses(document, token) {
        const lenses = [];

        for (let i = 0; i < document.lineCount; i++) {
            const lineText = document.lineAt(i).text;

            // crude match for your issue format: "- something [file::line]"
            const match = lineText.match(/\[(.+?)::(\d+)\]/);
            if (match) {
                const issueFile = match[1];
                const issueLine = parseInt(match[2], 10);

                // Put a CodeLens above the line
                const range = new vscode.Range(i, 0, i, 0);
                lenses.push(new vscode.CodeLens(range, {
                    title: `Jump to ${issueFile}:${issueLine}`,
                    command: 'taskpiea.jumpToIssue',
                    arguments: [issueFile, issueLine]
                }));
            }
        }

        return lenses;
    }
}