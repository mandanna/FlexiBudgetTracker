import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "./AuthContext";
import { Spinner } from "../components/Spinner";

export default function ProtectedRoute() {
  const { user, isLoading } = useAuth();

  if (isLoading) {
    return <Spinner />;
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}
