# Bender
A YAML driven binary file viewer

This program reads in a binary file specification written in YAML and emits neat, formatted data to your console.

1) Define your YAML spec
2) Feed YAML spec and a binary to Bender
3) Profit

![](examples/sample_01.PNG)

## YAML Definition
| Name | Default Value | Description |
|:-----|:--------------|:------------|
| format | bender.v1 | Version of this specification |
| name | _empty_ | Human friendly name of your spec |
| extensions | _empty_ | List of extensions associated with the file you are describing |
| base_element | Default Values for each type in Element | The default element for your spec |
| base_matrix | Default Value for each type in Matrix | The default matrix for your spec |
| matrices | _empty_ | A primitive type for arranging bytes in Row by Column format |
| structures | _empty_ | Complex data types, e.g. structs |
| elements | _empty_ | Named types of any of your custom types (matrices, deferreds, structures) |
| layout | _empty_ | Ordered list of elements as they are expected to be found in a binary. |

### Matrix Object
A matrix formatter is a definition for representing your data as a matrix. The name of the matrix can be referenced
by any element's matrix field. When a matrix is detected by the Bender parser, the format of the parent element controls the representation of the bytes. In the table below, the Units field controls the width of each variable in the array. The width parameter of the parent element control the *total* byte count fed into the matrix formatter.

| Field | Description | Legal Values |
|:------|:------------|:-------------|
| name | Name referenced by an element | strings |
| columns | How many variables per row | A positive integer |
| units | How many bytes per variable | A positive integer |

### Deferred Object
Sometimes your binary has dynamic data. We can still parse it by using a deferred definition. Using this approach, you are defining a contract that gives an offset (relative to the start of the file) and a size in bytes of future data. Once all the elements have been read, the deferred data types are processed according to your spec.

This pattern is based on the technique of inserting a marker in your binary that is effectively a custom pointer. Both the size and offset must be 4-byte values. There are no plans to support alternative integer sizes for these values.
```
typedef def_location_t {
	uint32_t size;
	uint32_t offset;
};
```

| Field | Description | Legal Values |
|:------|:------------|:-------------|
| name | Name referenced by an element | strings |
| size_bytes | Size in bytes of object | A positive integer |
| offset_bytes | Starting address of object | A positive integer |

### Structure
Sometimes your data is more than just a number or a matrix. Use structures to define sequences of bytes that create more complicated data types.

| Field | Description | Legal Values |
|:------|:------------|:-------------|
| name  | Name referenced by and element | strings |
| elements | A list of elements contained in this structure | Any valid Element listed under structure_elements |

### Element Object
An element is the name we use to describe one or more bytes in a binary file. It has a collection of fields that control how the bytes are read, stored, and displayed.

Before the definition of the elements list, there is defined a single element name base. This describes your spec by taking advantage of YAML merge keys.

| Field | Description | Legal Values |
|:------|:------------|:-------------|
| name | Name to display for this element | strings |
| elide | Hide this element from display | YAML bool |
| units | How many bytes are in this element | A positive integer |
| signed | Represent bytes as a signed value | YAML bool |
| format | How the bytes should be interpreted | binary, octal, decimal, hex, ascii, utf16, hexstr, float |
| little_endian | What order the bytes are stored in the file | YAML bool |
| matrix | Name of matrix formatter to use | Must be defined in your matrices list (see next section) |
| is_deferred | True if this object is a deferral | Optionally specify that this element is a pointer to more data |

