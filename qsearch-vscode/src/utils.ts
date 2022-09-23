import * as vscode from 'vscode';

export function hasWorkspaceFolder() : boolean {
  if (vscode.workspace.workspaceFolders && (vscode.workspace.workspaceFolders.length > 0)) {
    return true;
  }
  return false;
}

export function getWorkspaceFolder() {
  const workspaceRoot = (vscode.workspace.workspaceFolders && (vscode.workspace.workspaceFolders.length > 0))
  ? vscode.workspace.workspaceFolders[0].uri.fsPath : '';
  return workspaceRoot;
}

export function getNonce() {
  let text = '';
  const possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  for (let i = 0; i < 32; i++) {
    text += possible.charAt(Math.floor(Math.random() * possible.length));
  }
  return text;
}
