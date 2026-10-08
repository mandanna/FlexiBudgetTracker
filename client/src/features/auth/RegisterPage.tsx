import { useState } from "react";
import apiClient from "../../api/axiosClient";
import { Link, useNavigate } from "react-router-dom";
import FormField from "../../components/FormField";
import { getApiError } from "../../utils/apiErrors";

export default function RegisterPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [error, setError] = useState("");
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { id, value } = e.target;

    setForm((prev) => ({ ...prev, [id]: value }));
    setFieldErrors((prev) => ({
      ...prev,
      [id]: "",
    }));
  };

  async function registerUser() {
    setError("");
    try {
      await apiClient.post("/api/Login/register", form);
      navigate("/login");
    } catch (err) {
      const { message, fieldErrors } = getApiError(err);
      setError(message);
      setFieldErrors(fieldErrors);
    }
  }
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 ">
      <form
        className="bg-white rounded p-8 shadow flex flex-col border border-gray-100"
        onSubmit={(e) => {
          e.preventDefault();
          registerUser();
        }}
      >
        <h1 className="text-2xl font-bold text-gray-800 text-center">
          Create your account
        </h1>
        <p className="text-sm text-gray-500 mt-1 text-center mb-5">
          Plan every paycheck before you spend it
        </p>
        {error && (
          <p className="text-red-600 text-sm text-center mb-2">{error}</p>
        )}
        <div className="flex gap-2">
          <FormField
            id="firstName"
            label="First Name"
            type="text"
            placeholder="Enter your first name"
            value={form.firstName}
            error={fieldErrors.firstName}
            onChange={handleChange}
          />

          <FormField
            id="lastName"
            label="Last Name"
            type="text"
            placeholder="Enter your last name"
            value={form.lastName}
            error={fieldErrors.lastName}
            onChange={handleChange}
          />
        </div>
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
        <FormField
          id="confirmPassword"
          label="Confirm Password"
          type="password"
          placeholder="Confirm your password"
          value={form.confirmPassword}
          error={fieldErrors.confirmPassword}
          onChange={handleChange}
        />
        <div className="flex flex-col mb-2">
          <button
            type="submit"
            className="bg-blue-500 text-white px-4 py-2 rounded"
          >
            Register
          </button>
        </div>
        <p className="text-sm text-gray-600 flex justify-center gap-1">
          Already have an account?
          <Link to="/login" className="text-blue-600 hover:underline">
            Sign in
          </Link>
        </p>
      </form>
    </div>
  );
}
