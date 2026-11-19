import { useEffect, useState } from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { getToken } from "../api/client";

export default function RequireAuth() {
  const location = useLocation();
  const [authenticated, setAuthenticated] = useState(Boolean(getToken()));

  useEffect(() => {
    const sync = () => setAuthenticated(Boolean(getToken()));
    window.addEventListener("medireminder:session", sync);
    window.addEventListener("medireminder:session-expired", sync);
    return () => {
      window.removeEventListener("medireminder:session", sync);
      window.removeEventListener("medireminder:session-expired", sync);
    };
  }, []);

  return authenticated ? <Outlet /> : <Navigate to="/login" replace state={{ from: location }} />;
}
