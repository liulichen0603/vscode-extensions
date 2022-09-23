import * as vscode from 'vscode';
import axios from 'axios';

import { Log, LogLevel } from './log'

export class HttpUtils {
  public static httpInstance : HttpUtils | undefined;

  static async get(url: string) {
    try {
      const response = await axios.get(url);
      console.log(response);
    } catch (error) {
      Log(LogLevel.error, 'http get error: ' + error);
    }
  }

  static async post(url: string, data: any, responseBack : Function, errorBack : Function, userData : any) {
    try {
      const config = {
        headers: {
          'Content-Type': 'application/json',
          Accept: 'application/json',
        },
      };
      const response = await axios.post(url, data, config);
      responseBack(userData, response);
    } catch (error) {
      Log(LogLevel.error, 'http post error: ' + error);
      errorBack(userData, error);
    }
  }

  private static _initInstance() {
    if (!HttpUtils.httpInstance) {
      HttpUtils.httpInstance = new HttpUtils();
    }
  }

  private constructor() {}
}
