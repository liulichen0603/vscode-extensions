import * as vscode from 'vscode';
import { getNonce } from './utils';
import * as protobuf from 'protobufjs';

// let proto = "syntax=\"proto3\";\
// message MyMessage {\
//   string some_field = 1;\
// }";

// let root = protobuf.parse(proto, { keepCase: true }).root; // or use Root#load

// // converts a string from underscore notation to camel case
// function toCamelCase(str: string) {
//     return str.substring(0,1) + str.substring(1).replace(/_([a-z])(?=[a-z]|$)/g, function($0, $1) { return $1.toUpperCase(); });
// }

// // adds a virtual alias property
// function addAliasProperty(type : protobuf.Type, name : string, aliasName : string) {
//     if (aliasName !== name)
//         Object.defineProperty(type.ctor.prototype, aliasName, {
//             get: function() { return this[name]; },
//             set: function(value) { this[name] = value; }
//         });
// }

// // this function adds alternative getters and setters for the camel cased counterparts
// // to the runtime message's prototype (i.e. without having to register a custom class):
// function addVirtualCamelcaseFields(type : protobuf.Type) {
//     type.fieldsArray.forEach(function(field) {
//         addAliasProperty(type, field.name, toCamelCase(field.name));
//     });
//     type.oneofsArray.forEach(function(oneof) {
//         addAliasProperty(type, oneof.name, toCamelCase(oneof.name));
//     });
//     return type;
// }

// let messageObj = /** @type {protobuf.ReflectionObject | null} */ (root.lookup("MyMessage"));

// let MyMessage = addVirtualCamelcaseFields(messageObj);

// let myMessage = MyMessage.create({
//     someField /* or someField */: "hello world"
// });

// console.log(
//     "someField:", myMessage.someField,
//     "\nsome_field:", myMessage.some_field,
//     "\nJSON:", JSON.stringify(myMessage)
// );


export function pbTest(extensionUri: vscode.Uri) {
  protobuf.load(vscode.Uri.joinPath(extensionUri, 'proto', "sample.proto").fsPath, function(err, root) {
    if (err)
        throw err;
    if (!root) {
      return;
    }

    // Obtain a message type
    let SampleMessage = root.lookupType("samplepackage.SampleMessage");
  
    // Exemplary payload
    let payload = { sampleField: "SampleString" };
  
    // Verify the payload if necessary (i.e. when possibly incomplete or invalid)
    let errMsg = SampleMessage.verify(payload);
    if (errMsg)
        throw Error(errMsg);
  
    // Create a new message
    let message = SampleMessage.create(payload); // or use .fromObject if conversion is necessary
  
    // Encode a message to an Uint8Array (browser) or Buffer (node)
    let buffer = SampleMessage.encode(message).finish();
    // ... do something with buffer
  
    // Decode an Uint8Array (browser) or Buffer (node) to a message
    let message2 = SampleMessage.decode(buffer);
    // ... do something with message
  
    // If the application uses length-delimited buffers, there is also encodeDelimited and decodeDelimited.
  
    // Maybe convert the message back to a plain object
    let object = SampleMessage.toObject(message2, {
        longs: String,
        enums: String,
        bytes: String,
        // see ConversionOptions
    });
  });
}
