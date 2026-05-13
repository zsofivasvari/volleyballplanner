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

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;

    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
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
    <main
      className="auth-page auth-page-image"
      style={{ backgroundImage: "url('/images/login.png')" }}
    >
      <section className="auth-card">
        <h1>VolleyMind</h1>
        <p className="auth-subtitle">
          Strandröplabda edzéstervező és fejlődéskövető rendszer
        </p>

        <form onSubmit={handleSubmit}>
          <div className="auth-field">
            <label htmlFor="email">Email cím</label>
            <input
              id="email"
              name="email"
              type="email"
              placeholder="pelda@email.com"
              value={formData.email}
              onChange={handleInputChange}
              required
            />
          </div>

          <div className="auth-field">
            <label htmlFor="password">Jelszó</label>
            <input
              id="password"
              name="password"
              type="password"
              placeholder="••••••••"
              value={formData.password}
              onChange={handleInputChange}
              required
            />
          </div>

          {error && <p className="auth-error">{error}</p>}

          <button className="auth-button" type="submit" disabled={loading}>
            {loading ? "Bejelentkezés..." : "Bejelentkezés"}
          </button>
        </form>

        <Link to="/forgot-password" className="auth-link">
          Elfelejtetted a jelszavad?
        </Link>

        <p className="auth-bottom-text">
          Nincs még fiókod? <Link to="/register">Regisztráció</Link>
        </p>
      </section>
    </main>
  );
}

export default LoginPage;