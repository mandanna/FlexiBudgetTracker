import { Spinner } from "./Spinner";
export function LoadingOverlay({
  children,
  isLoading,
}: {
  children: React.ReactNode;
  isLoading: boolean;
}) {
  return (
    <div className="relative min-h-125">
      {isLoading && (
        <div className="absolute inset-0 bg-white/60 flex  items-center justify-center z-10">
          <Spinner />
        </div>
      )}
      <div className={isLoading ? "opacity-50 pointer-events-none" : ""}>
        {children}
      </div>
    </div>
  );
}
