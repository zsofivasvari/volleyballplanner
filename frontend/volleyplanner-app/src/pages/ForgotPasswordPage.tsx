import { useState } from "react";
import { AxiosError } from "axios";
import { Link } from "react-router-dom";
import api from "../api/axios";
import "../styles/auth.css";

function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    setMessage("");
    setError("");
    setLoading(true);

    try {
      const response = await api.post("/Auth/forgot-password", { email });
      setMessage(response.data.message);
    } catch (err: unknown) {
      if (err instanceof AxiosError) {
        setError(err.response?.data?.message || "Sikertelen művelet.");
      } else {
        setError("Sikertelen művelet.");
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
          <p>Elfelejtett jelszó</p>
        </div>

        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="auth-field">
            <label htmlFor="email">Email cím</label>
            <input
              id="email"
              type="email"
              placeholder="pelda@email.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          {message && <div className="auth-success">{message}</div>}
          {error && <div className="auth-error">{error}</div>}

          <button className="auth-button" type="submit" disabled={loading}>
            {loading ? "Küldés..." : "Visszaállítási link küldése"}
          </button>
        </form>

        <div className="auth-footer">
          <Link to="/">Vissza a bejelentkezéshez</Link>
        </div>
      </div>
    </div>
  );
}

export default ForgotPasswordPage;