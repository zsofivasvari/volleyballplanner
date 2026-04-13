type JwtPayload = {
  [key: string]: unknown;
  role?: string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string;
};

const parseJwt = (token: string): JwtPayload | null => {
  try {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split("")
        .map((char) => "%" + ("00" + char.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );

    return JSON.parse(jsonPayload) as JwtPayload;
  } catch {
    return null;
  }
};

export const getUserRole = (): string | null => {
  const token = localStorage.getItem("token");

  if (!token) {
    return null;
  }

  const payload = parseJwt(token);

  if (!payload) {
    return null;
  }

  return (
    (payload[
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ] as string | undefined) ||
    (payload.role as string | undefined) ||
    null
  );
};

export const isAdmin = (): boolean => {
  return getUserRole() === "Admin";
};