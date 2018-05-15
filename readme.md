# Bender
A YAML driven binary file viewer

## YAML Definition
| Name | Default Value | Description |
|:-----|:--------------|:------------|
| format | bender.v1 | Version of this specification |
| name | _empty_ | Human friendly name of your spec |
| extensions | _empty_ | List of extensions associated with the file you are describing |
| base | _empty_ | The default element for your spec |
| elements | _empty_ | Ordered list of elements in your file |
| matrices | _empty_ | Matrix data format spec |
| deferreds | _empty_ | List of dynamic data descriptors |

### Element Object
An element is the name we use to describe one or more bytes in a binary file. It has a collection of fields that control how the bytes are read, stored, and displayed.

Before the definition of the elements list, there is defined a single element name base. This describes your spec by taking advantage of YAML merge keys.
| Field | Description | Legal Values |
|:------|:------------|:-------------|
| little_endian | What order the bytes are stored in the file | YAML bool |
| elide | Hide this element from display | YAML bool |
| readonly | Allow editing this value | YAML bool |
| format | How the bytes should be interpretted | binary, octal, decimal, hex, ascii, utf16 |
| width | How many bytes are in this element | A positive integer |
| name | Name to display for this element | strings |
| matrix | Name of matrix formatter to use | Must be defined in your matrices list (see next section) |
| deferred | Name of deferred object | Optionally specifcy that this element is really a pointer to more data |
| signed | Represent bytes as a signed value | YAML bool |

### Matrix Object
A matrix formatter is a definition for representing your data as a matrix. The name of the matrix can be referenced
by any element's matrix field. When a matrix is detected by the Bender parser, the format of the parent element controls the representation of the bytes. In the table below, the Units field controls the width of each variable in the array. The width parameter of the parent element control the *total* byte count fed into the matrix formatter.
| Field | Description | Legal Values |
|:------|:------------|:-------------|
| name | Name referenced by an element | strings |
| columns | How many variables per row | A positive integer |
| Units | How many bytes per variable | A positive integer |

### Deferred Object
Sometimes your binary has dynamic data. We can still parse it by using a deferred definition. Using this approach, you are defining a contract that gives an offset (relative to the start of the file) and a size in bytes of future data. Once all the elements have been read, the deferred data types are processed according to your spec.

This pattern is based on the technique of inserting a marker in your binary that is effectively a custom pointer. You are free to use whichever data types
you would like for size and offset and long with the sum of their size in bytes is equal to the width specified in the parent element.
```
typedef def_location_t {
	uint32_t size;
	uint32_t offset;
};
```

| Field | Description | Legal Values |
|:------|:------------|:-------------|
| name | Name referenced by an element | strings |
| size_width | Width in bytes of size field | A positive integer |
| offset_width | Width in bytes of offset field | A positive integer |