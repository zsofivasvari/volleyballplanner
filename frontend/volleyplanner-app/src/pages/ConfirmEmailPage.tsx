import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { AxiosError } from "axios";
import api from "../api/axios";
import "../styles/auth.css";

function ConfirmEmailPage() {
  const [searchParams] = useSearchParams();
  const [message, setMessage] = useState("Email megerősítése folyamatban...");
  const [isError, setIsError] = useState(false);

  useEffect(() => {
    const confirmEmail = async () => {
      const token = searchParams.get("token");

      if (!token) {
        setMessage("Hiányzó token.");
        setIsError(true);
        return;
      }

      try {
        const response = await api.get(`/Auth/confirm-email?token=${token}`);
        setMessage(response.data.message);
        setIsError(false);
      } catch (err: unknown) {
        if (err instanceof AxiosError) {
          setMessage(
            err.response?.data?.message ||
              "Nem sikerült megerősíteni az email címet."
          );
        } else {
          setMessage("Nem sikerült megerősíteni az email címet.");
        }
        setIsError(true);
      }
    };

    void confirmEmail();
  }, [searchParams]);

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-brand">
          <h1>VolleyMind</h1>
          <p>Email megerősítés</p>
        </div>

        <div className={isError ? "auth-error" : "auth-success"}>
          {message}
        </div>

        <div className="auth-footer">
          <Link to="/">Vissza a bejelentkezéshez</Link>
        </div>
      </div>
    </div>
  );
}

export default ConfirmEmailPage;