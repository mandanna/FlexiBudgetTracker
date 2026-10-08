export default function EmptyTable({
  caption,
  description,
  buttonText,
  onClick,
}: {
  caption: string;
  description: string;
  buttonText: string;
  onClick: () => void;
}) {
  return (
    <div className="flex flex-col items-center justify-center p-8 text-center text-gray-500 border-2 border-dashed border-gray-300">
      <div className="rounded-full w-16 h-16 bg-blue-100 mb-4 items-center flex justify-center text-3xl text-blue-600">
        +
      </div>
      <div className="text-xl font-semibold text-gray-800">{caption}</div>
      <p className="text-gray-500 mt-2 max-w-md">{description}</p>
      <button
        className="bg-blue-600 text-white px-5 py-2.5 rounded font-medium text-sm mt-6"
        onClick={onClick}
      >
        {buttonText}
      </button>
    </div>
  );
}
