export function getRowErrors(
  fieldErrors: Record<string, string>,
  listName: string,
  rowIds: string[],
  fields: string[],
): Record<string, string> {
  const rowErrors: Record<string, string> = {};
  rowIds.forEach((rowId, index) => {
    for (const field of fields) {
      const message = fieldErrors[`${listName}[${index}].${field}`];
      if (message) rowErrors[`${field}-${rowId}`] = message;
    }
  });
  return rowErrors;
}