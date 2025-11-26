import { createFileRoute, useRouter } from "@tanstack/react-router";
import { useId, useState } from "react";
import { BlockyButton } from "@/components/ui/BlockyButton";
import { BlockyCard } from "@/components/ui/BlockyCard";
import { logIn } from "@/lib/api";

export const Route = createFileRoute("/login")({
	component: LoginComponent,
});

function LoginComponent() {
	const router = useRouter();
	const [formData, setFormData] = useState({ username: "", password: "" });
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState("");

	const handleSubmit = async (e: React.FormEvent) => {
		e.preventDefault();
		setIsLoading(true);
		setError("");

		try {
			await logIn(formData.username, formData.password);
			// Navigate to dashboard on success
			router.navigate({ to: "/" });
		} catch (_) {
			setError("INVALID_CREDENTIALS");
		} finally {
			setIsLoading(false);
		}
	};

	const username = useId();
	const password = useId();
	return (
		<div className="min-h-screen bg-slate-100 flex items-center justify-center p-4 font-sans">
			{/* BACKGROUND DECORATION (Optional Grid Pattern) */}
			<div
				className="absolute inset-0 opacity-[0.05] pointer-events-none"
				style={{
					backgroundImage: "radial-gradient(#000 1px, transparent 1px)",
					backgroundSize: "20px 20px",
				}}
			/>

			<div className="w-full max-w-md relative z-10">
				{/* HEADER BLOCK */}
				<div className="mb-6 text-center">
					<div
						className="inline-block bg-black text-white px-4 py-2 tracking-widest 
                            font-black text-2xl uppercase shadow-[4px_4px_0px_0px_rgba(253,224,71,1)]"
					>
						System_Access
					</div>
				</div>

				{/* LOGIN CARD */}
				<BlockyCard
					className={`p-8 bg-white transition-colors duration-200 ${error ? "border-red-500" : "border-black"}`}
				>
					<form onSubmit={handleSubmit} className="flex flex-col gap-6">
						{/* Username Input */}
						<div className="flex flex-col gap-2">
							<label
								htmlFor={username}
								className="font-black uppercase text-sm tracking-wide"
							>
								Username
							</label>
							<input
								id={username}
								type="text"
								required
								disabled={isLoading}
								className="border-4 border-black p-3 font-mono text-lg outline-none
                                        focus:shadow-[4px_4px_0px_0px_rgba(0,0,0,1)]
                                        transition-all duration-150 placeholder:text-gray-400
                                        disabled:opacity-50 disabled:cursor-not-allowed"
								placeholder="ENTER_ID..."
								value={formData.username}
								onChange={(e) =>
									setFormData({ ...formData, username: e.target.value })
								}
							/>
						</div>

						{/* Password Input */}
						<div className="flex flex-col gap-2">
							<label
								htmlFor={password}
								className="font-black uppercase text-sm tracking-wide"
							>
								Password
							</label>
							<input
								id={password}
								type="password"
								required
								disabled={isLoading}
								className="border-4 border-black p-3 font-mono text-lg outline-none
                                        focus:shadow-[4px_4px_0px_0px_rgba(0,0,0,1)]
                                        transition-all duration-150 placeholder:text-gray-400
                                        disabled:opacity-50 disabled:cursor-not-allowed"
								placeholder="••••••••"
								value={formData.password}
								onChange={(e) =>
									setFormData({ ...formData, password: e.target.value })
								}
							/>
						</div>

						{/* Error Message */}
						{error && (
							<div
								className="bg-red-500 text-white font-bold p-2 text-center border-2 border-black 
                                        uppercase text-sm animate-bounce shadow-[4px_4px_0px_0px_rgba(0,0,0,1)]"
							>
								⚠ Access Denied: {error}
							</div>
						)}

						{/* Submit Button */}
						<div className="mt-4">
							<BlockyButton
								type="submit"
								variant="primary"
								size="lg"
								className="w-full"
								disabled={isLoading}
							>
								{isLoading ? "Authenticating..." : "Log_In"}
							</BlockyButton>
						</div>
					</form>
				</BlockyCard>
			</div>
		</div>
	);
}
