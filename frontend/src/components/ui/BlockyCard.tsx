interface BlockyCardProps {
	children: React.ReactNode;
	className?: string;
	variant?: "white" | "yellow" | "blue";
}

export function BlockyCard({
	children,
	className = "",
	variant = "white",
}: BlockyCardProps) {
	const bgColors = {
		white: "bg-white",
		yellow: "bg-yellow-300",
		blue: "bg-blue-50",
	};

	return (
		<div
			className={`
        border-4 border-black 
        shadow-[8px_8px_0px_0px_rgba(0,0,0,1)] 
        ${bgColors[variant]} 
        ${className}
      `}
		>
			{children}
		</div>
	);
}
