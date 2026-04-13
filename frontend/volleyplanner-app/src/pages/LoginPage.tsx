import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { AxiosError } from "axios";
import { authService } from "../services/authService";
import "../styles/auth.css";

function LoginPage() {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    email: "",
    password: "",
  });

  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const result = await authService.login(formData);

      localStorage.setItem("token", result.token);
      localStorage.setItem("userName", result.name);
      localStorage.setItem("userEmail", result.email);

      navigate("/exercises");
    } catch (err: unknown) {
      if (err instanceof AxiosError) {
        setError(err.response?.data?.message || "Sikertelen bejelentkezés.");
      } else {
        setError("Sikertelen bejelentkezés.");
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
          <p>Strandröplabda edzéstervező és fejlődéskövető rendszer</p>
        </div>

        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="auth-field">
            <label htmlFor="email">Email cím</label>
            <input
              id="email"
              type="email"
              name="email"
              placeholder="pelda@email.com"
              value={formData.email}
              onChange={handleChange}
              required
            />
          </div>

          <div className="auth-field">
            <label htmlFor="password">Jelszó</label>
            <input
              id="password"
              type="password"
              name="password"
              placeholder="Add meg a jelszavad"
              value={formData.password}
              onChange={handleChange}
              required
            />
          </div>

          {error && <div className="auth-error">{error}</div>}

          <button className="auth-button" type="submit" disabled={loading}>
            {loading ? "Bejelentkezés..." : "Bejelentkezés"}
          </button>
        </form>

        <div style={{ textAlign: "right", marginTop: "0.75rem" }}>
          <Link to="/forgot-password">Elfelejtetted a jelszavad?</Link>
        </div>

        <div className="auth-footer">
          Nincs még fiókod? <Link to="/register">Regisztráció</Link>
        </div>
      </div>
    </div>
  );
}

export default LoginPage;