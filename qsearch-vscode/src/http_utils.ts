import * as vscode from 'vscode';
import axios from 'axios';

import { Log, LogLevel } from './log'

export class HttpUtils {
  public static httpInstance : HttpUtils | undefined;

  // type CreateUserResponse = {
  //   name: string;
  //   job: string;
  //   id: string;
  //   createdAt: string;
  // };
  
  // async static function postRequest() {
  //   try {
  //     // 👇️ const data: CreateUserResponse
  //     const { data } = await axios.post<CreateUserResponse>(
  //       'https://reqres.in/api/users',
  //       { name: 'John Smith', job: 'manager' },
  //       {
  //         headers: {
  //           'Content-Type': 'application/json',
  //           Accept: 'application/json',
  //         },
  //       },
  //     );
  
  //     console.log(JSON.stringify(data, null, 4));
  
  //     return data;
  //   } catch (error) {
  //     if (axios.isAxiosError(error)) {
  //       console.log('error message: ', error.message);
  //       // 👇️ error: AxiosError<any, any>
  //       return error.message;
  //     } else {
  //       console.log('unexpected error: ', error);
  //       return 'An unexpected error occurred';
  //     }
  //   }
  // }

  public static postData(url : string, param : any, callback : Function, catchback : Function) {
    HttpUtils._initInstance();
    axios.post(url, param)
      .then(function(response) {

      })
      .catch(function(error) {

      });
  }

  private static _initInstance() {
    if (!HttpUtils.httpInstance) {
      HttpUtils.httpInstance = new HttpUtils();
    }
  }

  private constructor() {}
}
