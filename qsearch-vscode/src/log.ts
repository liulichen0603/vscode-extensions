import * as vscode from 'vscode';

export enum LogLevel {
  info = 0,
  warn = 1,
  error = 2
}

class LogInternal {
  public static logInstance : LogInternal | undefined;

  public static LogOut(level : LogLevel, message : string) {
    if (!LogInternal.logInstance) {
      LogInternal.logInstance = new LogInternal();
    }
    let logMessage = '';
    switch (level) {
      case LogLevel.info:
        logMessage = `[info - ${LogInternal._now()}] ${message}`;
        vscode.window.showInformationMessage(logMessage);
        break;
      case LogLevel.warn:
        logMessage = `[warn - ${LogInternal._now()}] ${message}`;
        vscode.window.showWarningMessage(logMessage);
        break;
      case LogLevel.error:
        logMessage = `[error - ${LogInternal._now()}] ${message}`;
        vscode.window.showErrorMessage(logMessage);
        break;
      default:
        break;
    }
    console.log(logMessage);
  }

  private constructor() {}

  private static _now() {
      const now = new Date();
      return `${padLeft(`${now.getUTCHours()}`, 2, "0")}:${padLeft(`${now.getMinutes()}`, 2, "0")}:${padLeft(`${now.getUTCSeconds()}`, 2, "0")}.${now.getMilliseconds()}`;
  }
}

function padLeft(s : string, n : number, pad = " ") {
  return pad.repeat(Math.max(0, n - s.length)) + s;
}

export function Log(level : LogLevel, message : string) {
  LogInternal.LogOut(level, message);
}
