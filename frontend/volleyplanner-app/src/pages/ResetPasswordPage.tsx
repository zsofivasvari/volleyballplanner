import { useState } from "react";
import { AxiosError } from "axios";
import { Link, useSearchParams } from "react-router-dom";
import api from "../api/axios";
import "../styles/auth.css";

function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const [newPassword, setNewPassword] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    setMessage("");
    setError("");
    setLoading(true);

    const token = searchParams.get("token");

    if (!token) {
      setError("Hiányzó token.");
      setLoading(false);
      return;
    }

    try {
      const response = await api.post("/Auth/reset-password", {
        token,
        newPassword,
      });

      setMessage(response.data.message);
    } catch (err: unknown) {
      if (err instanceof AxiosError) {
        setError(
          err.response?.data?.message || "Nem sikerült jelszót módosítani."
        );
      } else {
        setError("Nem sikerült jelszót módosítani.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-brand">
          <h1>VolleyMind</h1>
          <p>Új jelszó beállítása</p>
        </div>

        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="auth-field">
            <label htmlFor="newPassword">Új jelszó</label>
            <input
              id="newPassword"
              type="password"
              placeholder="Adj meg új jelszót"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
            />
          </div>

          {message && <div className="auth-success">{message}</div>}
          {error && <div className="auth-error">{error}</div>}

          <button className="auth-button" type="submit" disabled={loading}>
            {loading ? "Mentés..." : "Jelszó módosítása"}
          </button>
        </form>

        <div className="auth-footer">
          <Link to="/">Vissza a bejelentkezéshez</Link>
        </div>
      </div>
    </div>
  );
}

export default ResetPasswordPage;