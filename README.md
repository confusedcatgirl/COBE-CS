### Bytecode Emulation (COBE)

This project aims for developers who want to include not-hardcoded userspace apps inside their cosmos os.

Cosmos OS Bytecode Emulation, or COBE for short will simplify the process, and help with this so that it will be easier.

## How to Run

``python emulator.py \<bin-file\>``

## Roadmap

- Adding full Emulation capabilities

- Including a simple "compiler"

- Rewriting the emulator into C#

- Including Docs on how to port it to the own COSMOS OS

## Limitations and Strengths

The Compiler tries to be very dynamic as possible. Any variable name without spaces is possible, excluding escape sequences, as they might cause problems. Instructions are case insensitive, and are recommended to be always written uppercase. This language does not support classes, or single line coding, as every instruction requires it's own line.

Another limitation is the space placement. COBE does not support intendation or double spaces, maybe in the future :P

Also, the emulator has to be rewritten for each cosmos project to some extend, as each os handles certain instructions a bit different.

## TODO

- File I/O Capabilities

- Midi Loading and outputting

- Beeping

- Screen Rendering

- Loops

- Ifs

- Multithreading

## Header

The Header ranges from Address 00000000 to 0000000F

| Address   | Description                       | Size |
|-----------|-----------------------------------|------|
| 0000 0000 | Mode of Program. 0 = CMD; 1 = GUI | 1    |
| 0000 0001 | Screen Width                      | 2    |
| 0000 0003 | Screen Height                     | 2    |

## List of Opcodes

| Hex  | Code | Description | Example | Binary Representation |
|------|------|-------------|---------|-----------------------|
| 0x00 | NOP | No Operation. Does nothing. | NOP | 00 |
| 0x01 | LBL | Label Start. | String hello: "Hello World!" | 01 01 68656C6C6F 00 2248656C6C6F20576F726C642122 |
| | | | Number num: 15 | 01 02 6E756D 00 000E |
| 0x02 | ADD | Adds the content of bb into aa. | ADD aa bb | 02 6161 00 6262 00 |
| 0x03 | ADV | Adds a value to a variable | ADV aa 10 | 03 6161 00 000A |
| 0x04 | SUB | Subtracts content of aa from bb. | SUB aa bb | 04 6161 00 6262 00 |
| 0x05 | SBV | Subtract value from a variable. | SBV aa 10 | 05 6161 00 000A |
| 0x06 | MUL | Multiply aa with bb. | MUL aa bb | 06 6161 00 6262 00 |
| 0x07 | MUV | Multiply a variable with a value. | MUV aa 10 | 07 6161 00 000A |
| 0x08 | DIV | Divide aa by bb. | DIV aa bb | 08 6161 00 6262 00 |
| 0x09 | DVV | Divide a variable with bb. | DVV aa 10 | 09 6161 00 000A |
| 0x0A | PUT | Write a string to the screen. | PUT hello | 10 01 68656C6C6F0A 00 |
| | | | PUT "Hello World!" | 10 02 2248656C6C6F20576F726C642122 00 |
| | | | PUT 1 | 10 03 0001 00 |
| 0x0B | RKI | Read Keyboard Input of Length X. | RKI 15 yourname | 11 0E 796F75726E616D650A |
| 0x0C | RET | Required Keyword to terminate. | RET 0 | 12 00 |
| 0x0D | MRK | Similar to Labels, but for jumps. | MRK marker01 | 13 6D61726B65723031 00 |
| 0x0E | JMP | Jumps to a specific marker. | JMP marker01 | 14 6D61726B65723031 00 |
| 0x0F | SSC | Sets the screen to w and h. | SSC 800 800 | 15 320 258 |
| 0x10 | BEP | Beeps anywhere, any time. | BEP | 16 |
| 0x11 | | | | |
| 0x12 | | | | |

## Creating Markers

Markers are used to jump back to specific positions inside the code. There are samples in the table above, but in code they can be used like this:

```
MRK loop
PUT "A loop!"
JMP loop
```

## Types

| Hex  | Definition     | Bytes used            |
|------|----------------|-----------------------|
| 0x00 | Boolean        | 1                     |
| 0x01 | String         | String Length         |
| 0x02 | Number         | 2                     |

## Operators

The Common Operators (>, <, >=, <=, !=, ==) can be used in If's and certain loop instructions. Please refer to the table for further information.