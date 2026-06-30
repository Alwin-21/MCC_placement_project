"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Mail, Lock } from "lucide-react";
import api from "@/services/api";

export default function AdminLoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !password) {
      alert("Please enter both admin email and password.");
      return;
    }
    try {
      setLoading(true);
      const response = await api.post("/Auth/admin-login", {
        email,
        password,
      });

      localStorage.setItem("admin", JSON.stringify(response.data.user));
      localStorage.setItem("adminToken", response.data.token);
      router.push("/admin");
    } catch (error: any) {
      alert(error?.response?.data?.message || "Invalid Admin Credentials");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#fcfaf6] text-[#2c2c2c] flex items-center justify-center p-6 relative overflow-hidden font-sans">
      
      {/* Decorative Branding Elements */}
      <div className="absolute top-0 left-0 w-full h-3.5 bg-[#781c1c] border-b border-amber-600/30" />

      <div className="w-full max-w-md space-y-6">
        
        {/* Back Link */}
        <Link href="/" className="inline-flex items-center gap-2 text-xs font-bold text-slate-500 hover:text-[#781c1c] mb-2 transition duration-200">
          <ArrowLeft size={14} /> Back to Home
        </Link>

        <div className="bg-white border border-slate-200 rounded-3xl p-10 shadow-lg">
          
          <div className="text-center mb-8">
            <div className="w-12 h-12 rounded-full bg-[#18233c] text-white flex items-center justify-center mx-auto mb-4 shadow">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="#d4af37" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" className="w-6 h-6">
                <circle cx="12" cy="5" r="3" />
                <line x1="12" y1="8" x2="12" y2="22" />
                <line x1="6" y1="12" x2="18" y2="12" />
                <path d="M5 12a7 7 0 0 0 14 0" />
              </svg>
            </div>
            
            <h1 className="font-serif text-3xl font-extrabold text-[#18233c] mb-1">
              Admin Portal
            </h1>
            <p className="text-xs text-slate-500 font-medium">
              Log in to manage placement records & student directories
            </p>
          </div>

          <form onSubmit={handleLogin} className="space-y-5">
            
            <div>
              <label className="text-[10px] uppercase font-mono tracking-wider font-bold text-slate-650 block mb-1.5">
                Admin Email
              </label>
              <div className="relative">
                <Mail className="absolute left-4 top-3.5 text-slate-400" size={16} />
                <input
                  type="email"
                  required
                  placeholder="admin@mcc.edu.in"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 focus:border-[#781c1c] text-slate-800 placeholder-slate-400 text-xs px-11 py-3.5 rounded-xl outline-none focus:ring-1 focus:ring-[#781c1c]/10 transition"
                />
              </div>
            </div>

            <div>
              <label className="text-[10px] uppercase font-mono tracking-wider font-bold text-slate-650 block mb-1.5">
                Secure Password
              </label>
              <div className="relative">
                <Lock className="absolute left-4 top-3.5 text-slate-400" size={16} />
                <input
                  type="password"
                  required
                  placeholder="Enter administrator password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 focus:border-[#781c1c] text-slate-800 placeholder-slate-400 text-xs px-11 py-3.5 rounded-xl outline-none focus:ring-1 focus:ring-[#781c1c]/10 transition"
                />
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full bg-[#18233c] hover:bg-slate-900 border border-amber-600/30 text-white font-bold text-xs uppercase tracking-wider py-3.5 rounded-xl transition duration-200 shadow shadow-black/10 active:scale-98 cursor-pointer"
            >
              {loading ? "Authenticating Admin..." : "Secure Log In"}
            </button>

          </form>

          <p className="text-center text-slate-400 mt-8 text-[10px] uppercase font-mono font-semibold tracking-wider">
            Madras Christian College · Official Access
          </p>

        </div>

      </div>

    </div>
  );
}