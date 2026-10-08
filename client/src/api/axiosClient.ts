import axios from 'axios'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,
})

// Response interceptor — on 401 (no session / expired), send the user to login
apiClient.interceptors.response.use(
  (response) => response,
(error) => {
    const isMeCheck = error.config?.url?.includes('/api/Login/me')
    if (error.response?.status === 401 && !isMeCheck) {
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
) 

export default apiClient

