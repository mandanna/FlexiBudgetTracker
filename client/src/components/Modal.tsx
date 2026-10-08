export default function Modal({
  isOpen,
  onClose,
  title,
  children,
}: {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
}) {
  if (!isOpen) return null;
  return (
    <div className="fixed inset-0 bg-gray-900/50 flex items-center justify-center z-50">
      <div className="relative bg-white p-6 rounded shadow-lg w-160">
        <h2 className="text-xl font-bold mb-4">{title}</h2>
        <button
          onClick={onClose}
          aria-label="Close"
          className="absolute top-2 right-3 text-2xl leading-none text-gray-500 hover:text-gray-700"
        >
          &times;
        </button>

        <div className="mb-4">{children}</div>
      </div>
    </div>
  );
}
