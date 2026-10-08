import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import apiClient from "../../api/axiosClient";
import { useAuth } from "../../auth/AuthContext";
import FormField from "../../components/FormField";
import { getApiError } from "../../utils/apiErrors";

export default function LoginPage() {
  const [form, setForm] = useState({
    email: "",
    password: "",
  });
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [message, setMessage] = useState("");
  const navigate = useNavigate();
  const { setUser } = useAuth();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setMessage("");

    const { id, value } = e.target;
    setForm((old) => ({ ...old, [id]: value }));
    setFieldErrors((old) => ({ ...old, [id]: "" }));
  };

  async function handleLogin() {
    setFieldErrors({});
    try {
      await apiClient.post("/api/Login", form);
      const me = await apiClient.get("/api/Login/Me");
      setUser(me.data.data);
      navigate("/paychecks");
    } catch (err) {
      const { message, fieldErrors } = getApiError(err);
      setMessage(message);
      setFieldErrors(fieldErrors);
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 ">
      <form
        onSubmit={(e) => {
          e.preventDefault();
          handleLogin();
        }}
        className="bg-white p-8 rounded shadow flex flex-col w-96 border border-gray-100"
      >
        <p className="text-3xl font-bold text-center mb-5">Flexi Budget</p>

        {message && (
          <p className="text-red-600 text-sm text-center mb-2">{message}</p>
        )}

        <FormField
          id="email"
          label="Email"
          type="email"
          placeholder="Enter your email"
          value={form.email}
          error={fieldErrors.email}
          onChange={handleChange}
        />
        <FormField
          id="password"
          label="Password"
          type="password"
          placeholder="Enter your password"
          value={form.password}
          error={fieldErrors.password}
          onChange={handleChange}
        />

        <button
          type="submit"
          className="bg-blue-600 text-white py-2 rounded hover:bg-blue-700 cursor-pointer mb-2"
        >
          Sign in
        </button>
        <label className="text-sm text-gray-600 flex justify-center gap-1">
          Don't have an account?
          <Link to="/register" className="text-blue-600 hover:underline">
            Register
          </Link>
        </label>
      </form>
    </div>
  );
}
