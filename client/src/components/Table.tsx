export function Table({
  header,
  body,
}: {
  header: React.ReactNode;
  body: React.ReactNode;
}) {
  return (
    <div className="rounded-lg border border-gray-200">
      <table className="w-full border border-gray-300 border-collapse text-sm">
        <thead className="bg-gray-100 text-gray-600">{header}</thead>
        <tbody>{body}</tbody>
      </table>
    </div>
  );
}
