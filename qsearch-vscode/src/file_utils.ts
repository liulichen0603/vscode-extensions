import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';

import { Log, LogLevel } from './log'

export class FileUtils {
  public static fileInstance : FileUtils | undefined;

  public static readFileSync(filePath : string) : string {
    FileUtils._initInstance();
    filePath = path.normalize(filePath);
    let data = '';
    try {
      data = fs.readFileSync(filePath, 'utf-8');
    } catch (err) {
      Log(LogLevel.error, 'getFileData error: ' + err);
    }
    return data;
  }

  public static copyFile(src : string, dst : string, callback : Function) {
    FileUtils._initInstance();
    fs.copyFile(src, dst, function(err) {
      if (err) throw err;
      if (callback) {
        callback();
      }
    });
  }

  private static _initInstance() {
    if (!FileUtils.fileInstance) {
      FileUtils.fileInstance = new FileUtils();
    }
  }

  private constructor() {}
}
