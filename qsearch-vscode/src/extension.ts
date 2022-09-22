// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';

import { Log, LogLevel } from './log'
import * as utils from './utils';
import { cpTest } from './cp_utils';
import { pbTest } from './pb_utils';
import { FileUtils } from './file_utils'
import { HttpUtils } from './http_utils'

export function activate(context: vscode.ExtensionContext) {
	Log(LogLevel.info, 'Congratulations, your extension "qsearch" is now active!');
	context.subscriptions.push(
		vscode.commands.registerCommand('QSearch.StartSearch', () => {
			QSearchViewPanel.createOrShow(context.extensionUri);
			// pbTest(context.extensionUri);
			// cpTest(context.extensionUri);
			HttpUtils.get('www.d.com')
				.then(function (response) {

				})
				.catch(function (error) {

				});
			HttpUtils.post('www.d.com', {p1: 1})
				.then(function(response) {

				})
				.catch(function(error) {

				});
		})
	);

	if (vscode.window.registerWebviewPanelSerializer) {
		// Make sure we register a serializer in activation event
		vscode.window.registerWebviewPanelSerializer(QSearchViewPanel.viewType, {
			async deserializeWebviewPanel(webviewPanel: vscode.WebviewPanel, state: any) {
				Log(LogLevel.info, `Got state: ${state}`);
				// Reset the webview options so we use latest uri for `localResourceRoots`.
				webviewPanel.webview.options = getWebviewOptions(context.extensionUri);
				QSearchViewPanel.revive(webviewPanel, context.extensionUri);
			}
		});
	}
}

function getWebviewOptions(extensionUri: vscode.Uri): vscode.WebviewOptions {
	return {
		// Enable javascript in the webview
		enableScripts: true,
		// And restrict the webview to only loading content from our extension's `resources` directory.
		localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'resources')]
	};
}

class QSearchViewPanel {
	// Track the current panel. Only allow a single panel to exist at a time.
	public static currentPanel: QSearchViewPanel | undefined;

	public static readonly viewType = 'QSearch';

	private readonly _panel: vscode.WebviewPanel;
	private readonly _extensionUri: vscode.Uri;
	private _disposables: vscode.Disposable[] = [];

	public static createOrShow(extensionUri: vscode.Uri) {
		const column = vscode.window.activeTextEditor ? vscode.window.activeTextEditor.viewColumn : undefined;

		if (QSearchViewPanel.currentPanel) {
			QSearchViewPanel.currentPanel._panel.reveal(column);
			return;
		}

		const panel = vscode.window.createWebviewPanel(QSearchViewPanel.viewType, 'Quick Search', column || vscode.ViewColumn.One, getWebviewOptions(extensionUri));
		QSearchViewPanel.currentPanel = new QSearchViewPanel(panel, extensionUri);
	}

	public static revive(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
		QSearchViewPanel.currentPanel = new QSearchViewPanel(panel, extensionUri);
	}

	private constructor(panel: vscode.WebviewPanel, extensionUri: vscode.Uri) {
		this._panel = panel;
		this._extensionUri = extensionUri;

		// Set the webview's initial html content.
		this._updateForWebview(this._panel.webview);

		// Listen for when the panel is disposed.
		// This happens when the user closes the panel or when the panel is closed programmatically.
		this._panel.onDidDispose(() => this.dispose(), null, this._disposables);

		// Update the content based on view changes.
		this._panel.onDidChangeViewState(e => {
				if (this._panel.visible) {
					this._updateForWebview(this._panel.webview);
				}
			}, null, this._disposables);

		// Handle messages from the webview
		this._panel.webview.onDidReceiveMessage(message => {
			Log(LogLevel.info, 'onDidReceiveMessage command: ' + message.command + ', text: ' + message.text.toString());
			switch (message.command) {
				case 'StartSearch':
					const fileName = 'utils.ts';
					const filePath = 'src\\utils.ts';
					const fileData = FileUtils.readFileSync(vscode.Uri.joinPath(this._extensionUri, filePath).fsPath);
					const count = '1';
					let text = [{
						fileName: fileName,
						filePath: filePath,
						count: count,
						fileData: fileData
					}];
					this.sendMessageToWebview('ShowResult', text);
					return;
			}
		}, null, this._disposables);
	}

	public sendMessageToWebview(command: string, text: any) {
		// Send a message to the webview webview.
		Log(LogLevel.info, 'sendMessageToWebview command: ' + command + ', text: ' + text.toString());
		this._panel.webview.postMessage({command: command, text: text});
	}

	public dispose() {
		QSearchViewPanel.currentPanel = undefined;

		// Clean up resources.
		this._panel.dispose();
		while(this._disposables.length) {
			const x = this._disposables.pop();
			if (x) {
				x.dispose();
			}
		}
	}

	private _updateForWebview(webview: vscode.Webview) {
		this._panel.title = 'QSearch';
		this._panel.webview.html = this._getHtmlForWebview(webview, 'https://media.giphy.com/media/JIX9t2j0ZTN9S/giphy.gif');
	}

	private _getHtmlForWebview(webview: vscode.Webview, gifPath: string) {
		// Local path to main script run in the webview.
		const scriptPath = vscode.Uri.joinPath(this._extensionUri, 'resources', 'main.js');
		// And the uri we use to load this script in the webview.
		const scriptUri = webview.asWebviewUri(scriptPath);

		// Local path to css styles.
		const stylesResetPath = vscode.Uri.joinPath(this._extensionUri, 'resources', 'reset.css');
		const stylesMainPath = vscode.Uri.joinPath(this._extensionUri, 'resources', 'vscode.css');
		// Uri to load styles into webview
		const stylesResetUri = webview.asWebviewUri(stylesResetPath);
		const stylesMainUri = webview.asWebviewUri(stylesMainPath);

		// Use a nonce to only allow specific scripts to be run
		const nonce = getNonce();

		return `<!DOCTYPE html>
			<html lang="en">
			<head>
				<meta charset="UTF-8">
				<!--
					Use a content security policy to only allow loading images from https or from our extension directory,
					and only allow scripts that have a specific nonce.
				-->
				<meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src ${webview.cspSource}; img-src ${webview.cspSource} https:; script-src 'nonce-${nonce}';">
				<meta name="viewport" content="width=device-width, initial-scale=1.0">
				<link href="${stylesResetUri}" rel="stylesheet">
				<link href="${stylesMainUri}" rel="stylesheet">
				<title>Quick Search</title>
			</head>
			<body>
				<div id="content"></div>
				<script nonce="${nonce}" src="${scriptUri}"></script>
			</body>
			</html>`;
	}
}

function getNonce() {
	let text = '';
	const possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
	for (let i = 0; i < 32; i++) {
		text += possible.charAt(Math.floor(Math.random() * possible.length));
	}
	return text;
}

