import { NavLink, Outlet, useNavigate } from "react-router-dom";
import apiClient from "../api/axiosClient";
import { useAuth } from "../auth/AuthContext";

export default function AppLayout() {
  const navigate = useNavigate();
  const { user, setUser } = useAuth();
  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `px-3 py-2 rounded ${isActive ? "bg-blue-100 text-blue-700" : "text-gray-600 hover:bg-gray-100"}`;
  async function handleLogout() {
    try {
      await apiClient.post("/api/Login/Logout");
    } finally {
      setUser(null);
      navigate("/login");
    }
  }

  return (
    <div className="flex min-h-screen">
      <aside className="w-60 border-r bg-gray-50 p-4 flex flex-col gap-1">
        <span className="font-semibold text-lg mb-6">FlexiBudget</span>
        <NavLink to="/dashboard" className={linkClass}>
          Dashboard
        </NavLink>
        <NavLink to="/paychecks" className={linkClass}>
          Paychecks
        </NavLink>
        <NavLink to="" className={linkClass}>
          Expenses
        </NavLink>
        <NavLink to="" className={linkClass}>
          Category
        </NavLink>
        <button
          onClick={handleLogout}
          className="mt-auto text-left px-3 py-2 rounded text-gray-600 hover:bg-gray-100"
        >
          Log out{user ? ` (${user.firstName})` : ""}
        </button>
      </aside>
      <main className="flex-1 p-8">
        <Outlet />
      </main>
    </div>
  );
}
