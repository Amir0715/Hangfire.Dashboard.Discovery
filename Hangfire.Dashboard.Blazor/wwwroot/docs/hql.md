# HQL - Hangfire Query Language

## Query Syntax

`[field] [operator] [value]`

Or reverse order:

`[value] [operator] [field]`

## Operators

`==` - equality  
`!=` - inequality  
`>` - greater than  
`>=` - greater than or equal  
`<` - less than  
`<=` - less than or equal  
`~=` - contains substring

## Logical Operators

`&&` - logical AND  
`||` - logical OR

## Grouping

Use parentheses for grouping conditions:

`([condition1]) || [condition2]`

## Value Types

- Strings: enclosed in quotes ("string")
- Numbers: without quotes (3.5)
- Dates: ISO 8601 format in quotes ("2025-05-29T12:00:00Z")

## Supported Fields

- Method
- State
- Id
- Queue
- Type
- Args (with nested fields using dot notation, e.g., Args.name, Args.customer.id)
- CreatedAt
- ExpireAt

## Examples

### Search by job type:

`Type == "ScheduleEventHandleJob"`

### Search by creation date:

`CreatedAt >= 2025-05-29T12:00:00Z`

### Search by argument value:

`Args.name == "ScheduleEventHandleJob"`

### Search by numeric argument:

`Args.Number >= 5.1`

### Search by date in arguments:

`Args.CreatedAt < "2025-05-28T13:00:00Z"`

### Combined conditions:

`(Type == "ScheduleEventHandleJob") || Type == "ScheduleEventHandleJob2"`

### Using contains operator:

`Type ~= "2"`

### Advanced JSON Querying

Access nested properties in JSON arguments using dot notation:

`Args.customer.id == "12345"`