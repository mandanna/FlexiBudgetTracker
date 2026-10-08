import {
  createContext,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";
import apiClient from "../api/axiosClient";
type User = {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
};

type AuthContextType = {
  user: User | null;
  isLoading: boolean;
  setUser: (user: User | null) => void;
};
const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    apiClient
      .get("/api/Login/me")
      .then((res) => {
        console.log(res.data.data);
        setUser(res.data.data);
      })
      .catch(() => setUser(null))
      .finally(() => setIsLoading(false));
  }, []);
  return (
    <AuthContext.Provider value={{ user, isLoading, setUser }}>
      {children}
    </AuthContext.Provider>
  );
}
export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
