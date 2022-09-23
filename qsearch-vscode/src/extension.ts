// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
import * as path from 'path';

import { Log, LogLevel } from './log'
import * as utils from './utils';
import * as cpUtils from './cp_utils';
import { pbTest } from './pb_utils';
import { FileUtils } from './file_utils'
import { HttpUtils } from './http_utils'

const gServerPort = 63303;

export function activate(context: vscode.ExtensionContext) {
  Log(LogLevel.info, 'Congratulations, your extension "qsearch" is now active!');
  context.subscriptions.push(
    vscode.commands.registerCommand('QSearch.StartSearch', () => {
      QSearchViewPanel.createOrShow(context.extensionUri);
      copyServerConfig(context.extensionUri);
      startServer(context.extensionUri);
    })
  );

  context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(function (event) {
    // todo: notify the server the file is saved
    Log(LogLevel.info, `file saved!`);
  }));

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

function copyServerConfig(extensionUri : vscode.Uri) {
  if (utils.hasWorkspaceFolder()) {
    let src : string = vscode.Uri.joinPath(extensionUri, 'resources', 'vs-chromium-project.txt').fsPath;
    let dst : string = path.join(utils.getWorkspaceFolder(), 'vs-chromium-project.txt');
    FileUtils.copyFile(src, dst, function(){
      Log(LogLevel.info, 'copyServerConfig success');
    });
  } else {
    Log(LogLevel.warn, 'copyServerConfig no workspace folder opened');
  }
}

function startServer(extensionUri : vscode.Uri) {
  if (utils.hasWorkspaceFolder()) {
    let command : string = 'start';
    let args : Array<string> = [
      vscode.Uri.joinPath(extensionUri, 'resources', 'server', 'MyTest.exe').fsPath,
      utils.getWorkspaceFolder(),
      gServerPort.toString()
    ];
    cpUtils.runAsync(command, args);

    args = [
      vscode.Uri.joinPath(extensionUri, 'resources', 'server', 'VsChromium.Server.exe').fsPath,
      '63301'
    ];
    cpUtils.runAsync(command, args);
  } else {
    Log(LogLevel.warn, 'startServer no workspace folder opened');
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
  private _initialized = false;

  public static createOrShow(extensionUri: vscode.Uri) {
    const column = vscode.window.activeTextEditor ? vscode.window.activeTextEditor.viewColumn : undefined;

    if (QSearchViewPanel.currentPanel) {
      QSearchViewPanel.currentPanel._panel.reveal(column);
      return;
    }

    const panel = vscode.window.createWebviewPanel(
      QSearchViewPanel.viewType, 'Quick Search', column || vscode.ViewColumn.Two, 
      {
        // Enable javascript in the webview
        enableScripts: true,
        retainContextWhenHidden: true,
        // And restrict the webview to only loading content from our extension's `resources` directory.
        localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'resources')]
      });
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
          HttpUtils.post('http://127.0.0.1:63303/', JSON.stringify({
            'SearchString': message.text.searchString,
            'SearchPath': message.text.searchFile
          }), function(userData : any, response : any) {
            Log(LogLevel.info, 'StartSearch response');
            let cls : QSearchViewPanel = userData;
            if (!response.data.IsEmpty) {
              let text = [];
              let entries = response.data.Entries;
              for (let i = 0; i < entries.length; i++) {
                let filePath = entries[i].Name;
                let fileName = path.basename(entries[i].Name);
                let count = entries[i].Data.Positions.length;
                let fileData = FileUtils.readFileSync(path.normalize(path.join(utils.getWorkspaceFolder(), filePath)));
                text.push({
                  fileName: fileName,
                  filePath: filePath,
                  count: count,
                  fileData: fileData
                });
              }
              cls.sendMessageToWebview('ShowResult', text);
            } else {
              cls.sendMessageToWebview('ShowResult', []);
            }
          }, function(userData : any, error : any) {
            Log(LogLevel.error, 'StartSearch error');
          }, this);
          break;
        case 'OpenFileInVSCode':
          vscode.workspace.openTextDocument(path.join(utils.getWorkspaceFolder(), message.text))
            .then((document) => {
              vscode.window.showTextDocument(document, undefined, true);
            });
          break;
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
    if (!this._initialized){
      this._panel.title = 'QSearch';
      this._panel.webview.html = this._getHtmlForWebview(webview, 'https://media.giphy.com/media/JIX9t2j0ZTN9S/giphy.gif');
      this._initialized = true;
    }
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
    const nonce = utils.getNonce();

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

