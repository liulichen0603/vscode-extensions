import * as vscode from 'vscode';
import * as cp from 'child_process';
import { error } from 'console';

import { Log, LogLevel } from './log'

function exec(command: string, options: cp.ExecOptions): Promise<{ stdout: string; stderr: string }> {
  return new Promise<{ stdout: string; stderr: string }>((resolve, reject) => {
    cp.exec(command, options, (error, stdout, stderr) => {
      if (error) {
        reject({ error, stdout, stderr });
      }
      resolve({ stdout, stderr });
    });
  });
}

let _channel: vscode.OutputChannel;
function getOutputChannel(): vscode.OutputChannel {
  if (!_channel) {
    _channel = vscode.window.createOutputChannel('Output Channel');
  }
  return _channel;
}

// export async function cpTest() {
//   try {
//     const { stdout, stderr } = await exec('echo test', { cwd: 'folderString' });
//     if (stderr && stderr.length > 0) {
//       getOutputChannel().appendLine(stderr);
//       getOutputChannel().show(true);
//     }
//     if (stdout) {
//       console.log('stdout: ', stdout);
//     }
//   } catch (err: any) {
//     const channel = getOutputChannel();
//     if (err.stderr) {
//       channel.appendLine(err.stderr);
//     }
//     if (err.stdout) {
//       channel.appendLine(err.stdout);
//     }
//     channel.appendLine('execute command failed.');
//     channel.show(true);
//   }
// }

export function cpTest(extensionUri : vscode.Uri): String {
  let testExePath = vscode.Uri.joinPath(extensionUri, 'resources', "test.exe").fsPath
  const proc = cp.spawnSync(testExePath, {
    shell: true,
    encoding: 'utf8'
  });

  let procData = proc.stdout.toString();

  if (proc !== null) {
    if (proc.stdout !== null && proc.stdout.toString() !== '') {
      procData = proc.stdout.toString();
      Log(LogLevel.info, "The '" + testExePath + "' process success: " + procData);
    }
    if (proc.stderr !== null && proc.stderr.toString() !== '') {
      const procErr = proc.stderr.toString;
      Log(LogLevel.error, "The '" + testExePath + "' process failed: " + procErr);
    }
  }

  return procData;
}

export function runAsync(command : string, args : Array<string>) : string {
  const proc = cp.spawn(command, args, {
    shell: true
  });

  let procData = proc.stdout.toString();

  if (proc !== null) {
    if (proc.stdout !== null && proc.stdout.toString() !== '') {
      procData = proc.stdout.toString();
      Log(LogLevel.info, "The '" + command + "' process success: " + procData);
    }
    if (proc.stderr !== null && proc.stderr.toString() !== '') {
      const procErr = proc.stderr.toString;
      Log(LogLevel.error, "The '" + command + "' process failed: " + procErr);
    }
  }

  return procData;
}
