import { Routes, Route, Navigate } from "react-router-dom";
import ProtectedRoute from "./auth/ProtectedRoute";
import AppLayout from "./components/AppLayout";
import LoginPage from "./features/auth/LoginPage";
import RegisterPage from "./features/auth/RegisterPage";
import PaychecksPage from "./features/paychecks/PaychecksPage";
import DashboardPage from "./features/dashboard/DashboardPage";

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/paychecks" element={<PaychecksPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/login" />} />
      <Route path="/register" element={<RegisterPage />} />
    </Routes>
  );
}

export default App;
