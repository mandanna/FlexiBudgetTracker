export default function FormField({
  id,
  label,
  type,
  placeholder,
  value,
  error,
  labelClassName,
  onChange,
}: {
  id: string;
  label: string;
  type: string;
  placeholder: string;
  value: any;
  error?: string;
  labelClassName?: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}) {
  return (
    <div className="flex flex-col mb-2">
      <label
        htmlFor={id}
        className={`font-extralight text-gray-500 mb-1 ${labelClassName ?? "text-sm "}`}
      >
        {label}
      </label>
      <input
        id={id}
        type={type}
        value={value}
        placeholder={placeholder}
        onChange={onChange}
        className={`text-black  border rounded px-3 py-2 ${error ? "border-red-500" : ""}`}
      />
      {error && <p className="text-red-500 text-xs mt-1">{error}</p>}
    </div>
  );
}
