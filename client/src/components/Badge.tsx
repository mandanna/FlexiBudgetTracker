export type BadgeVariant = "success" | "neutral" | "info" | "purple";

const variantClasses: Record<BadgeVariant, string> = {
  success: "bg-green-100 text-green-700",
  neutral: "bg-gray-200 text-gray-600",
  info: "bg-blue-50 text-blue-700",
  purple: "bg-purple-50 text-purple-700",
};

export function Badge({
  variant,
  children,
}: {
  variant: BadgeVariant;
  children: React.ReactNode;
}) {
  let className = variantClasses[variant];

  return (
    <span
      className={`text-center rounded-full px-2 py-0.5 text-xs font-medium ${className}`}
    >
      {children}
    </span>
  );
}
