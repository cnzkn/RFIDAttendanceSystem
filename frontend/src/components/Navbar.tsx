import { Link } from "@tanstack/react-router";
import { useEffect, useId, useState } from "react";
import { BlockyButton } from "@/components/ui/BlockyButton";
import { BlockyCard } from "@/components/ui/BlockyCard";
import { logOut } from "@/lib/api";

export function Navbar() {
	const userButtonId = useId();
	const userMenuId = useId();

	// We need state to trigger a re-render for the rotation animation
	const [isOpen, setIsOpen] = useState(false);

	useEffect(() => {
		const popoverElement = document.getElementById(userMenuId);

		if (!popoverElement) return;

		// The 'beforetoggle' event is the native way to detect Popover API changes
		const handleToggle = (event: ToggleEvent) => {
			setIsOpen(event.newState === "open");
		};

		popoverElement.addEventListener(
			"beforetoggle",
			handleToggle as EventListener,
		);

		return () => {
			popoverElement.removeEventListener(
				"beforetoggle",
				handleToggle as EventListener,
			);
		};
	}, [userMenuId]);

	return (
		<nav className="sticky top-0 z-50 flex justify-between items-center bg-white border-b-4 border-black px-6 py-4">
			{/* LEFT SIDE: Brand & Navigation */}
			<ul className="flex flex-row items-center gap-2">
				{/* Brand Logo */}
				<li className="flex h-full">
					<Link
						to="/"
						className="flex items-center gap-3 text-2xl font-black uppercase tracking-tighter hover:text-blue-700 transition-colors group"
					>
						{/* The Logo Icon */}
						<div className="w-8 h-8 bg-black text-white flex items-center justify-center text-sm border-2 border-transparent group-hover:bg-blue-600 group-hover:border-black transition-colors">
							R
						</div>
						RFID_SYS
					</Link>
				</li>

				{/* Reusable Divider Component */}
				<NavDivider />

				{/* Navigation Links */}
				<li className="flex h-full">
					<Link
						to="/"
						className="font-bold uppercase tracking-wide px-3 py-1 border-2 border-transparent hover:border-black hover:bg-yellow-300 transition-all"
						activeProps={{
							className: "bg-yellow-300 border-black", // Active state style
						}}
					>
						Weekly Schedule
					</Link>
				</li>
				<li className="flex h-full">
					<Link
						to="/courses"
						className="font-bold uppercase tracking-wide px-3 py-1 border-2 border-transparent hover:border-black hover:bg-yellow-300 transition-all"
						activeProps={{
							className: "bg-yellow-300 border-black", // Active state style
						}}
					>
						All Courses
					</Link>
				</li>
			</ul>

			{/* RIGHT SIDE: User Action */}
			<BlockyButton
				popoverTarget={userMenuId}
				id={userButtonId}
				variant="neutral"
				size="md"
				className="flex items-center gap-2 group"
			>
				<span>User</span>
				<span
					className={`text-[10px] transition-transform duration-200 ${isOpen ? "rotate-180" : ""}`}
				>
					▼
				</span>
			</BlockyButton>

			{/* DROPDOWN MENU POPOVER */}
			<div
				popover="auto"
				id={userMenuId}
				className="bg-transparent p-0 border-none outline-none overflow-visible m-0"
				style={{
					positionAnchor: `anchor(${userButtonId})`,
					inset: "auto",
					top: "anchor(bottom)",
					right: "anchor(right)",
					marginTop: "12px",
				}}
			>
				{/* reusing BlockyCard for the dropdown container */}
				<BlockyCard className="min-w-[180px] p-0 flex flex-col shadow-[6px_6px_0px_0px_rgba(0,0,0,1)]">
					{/* Menu Header */}
					<div className="bg-black text-white text-[10px] font-mono px-3 py-1 uppercase tracking-widest border-b-2 border-black">
						System Menu
					</div>

					{/* Menu Actions - Using standard button for "List Item" feel */}
					<button
						type="button"
						onClick={() => {
							logOut();
							window.location.href = "/";
						}}
						className="group w-full text-left flex justify-between items-center px-4 py-3 font-bold uppercase 
                            text-sm bg-white hover:bg-red-500 hover:text-white transition-colors focus:outline-none"
					>
						Log_out
						<span className="text-lg mr-1 group-hover:translate-x-2 transition-transform">
							→
						</span>
					</button>
				</BlockyCard>
			</div>
		</nav>
	);
}

// Small helper component for that slanted line
function NavDivider() {
	return (
		<li className="h-8 w-[3px] bg-black skew-x-[-15deg] opacity-20 mx-4" />
	);
}
