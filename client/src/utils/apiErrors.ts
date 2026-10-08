import axios from "axios";
export type ApiError={
    message: string;
    fieldErrors: Record<string, string>;
}


export function getApiError(err: unknown): ApiError {
if (!axios.isAxiosError(err)|| !err.response) {
  return { message: "Something went wrong. Please try again.", fieldErrors: {} };
}
const body = err.response.data;
if (body.data && typeof body.data === "object") {
  const fieldErrors: Record<string, string> = {};
  for (const [path, messages] of Object.entries(body.data as Record<string, string[]>)) {
    fieldErrors[path] = messages[0];
  }
  return { message: body.message, fieldErrors };
}

return { message: body.message, fieldErrors: {} };
   
}