interface BlockyButtonProps
	extends React.ButtonHTMLAttributes<HTMLButtonElement> {
	variant?: "primary" | "success" | "danger" | "neutral" | "ghost";
	size?: "sm" | "md" | "lg";
}

export function BlockyButton({
	children,
	variant = "primary",
	size = "md",
	className = "",
	...props
}: BlockyButtonProps) {
	const baseStyles = `
    font-black uppercase tracking-wide border-2 border-black 
    transition-all duration-200 ease-out
    
    hover:-translate-y-[2px] hover:-translate-x-[2px]
    
    active:!translate-x-[2px] active:!translate-y-[2px] 
    active:shadow-none
    active:duration-75
  `;

	const variants = {
		primary:
			"bg-blue-600 text-white hover:bg-blue-500 shadow-[4px_4px_0px_0px_rgba(0,0,0,1)] hover:shadow-[6px_6px_0px_0px_rgba(0,0,0,1)]",
		success:
			"bg-green-400 text-black hover:bg-green-300 shadow-[3px_3px_0px_0px_rgba(0,0,0,1)] hover:shadow-[5px_5px_0px_0px_rgba(0,0,0,1)]",
		danger:
			"bg-red-500 text-white hover:bg-red-400 shadow-[3px_3px_0px_0px_rgba(0,0,0,1)] hover:shadow-[5px_5px_0px_0px_rgba(0,0,0,1)]",
		neutral:
			"bg-white text-gray-900 hover:bg-gray-50 shadow-[4px_4px_0px_0px_rgba(0,0,0,1)] hover:shadow-[6px_6px_0px_0px_rgba(0,0,0,1)]",

		// Ghost buttons stay flat
		ghost:
			"bg-transparent border-transparent shadow-none text-gray-500 hover:text-black hover:bg-gray-100 translate-none hover:translate-none active:translate-none",
	};

	const sizes = {
		sm: "px-3 py-1 text-xs",
		md: "px-6 py-2 text-sm",
		lg: "px-8 py-4 text-lg",
	};

	const isGhost = variant === "ghost";
	const appliedBase = isGhost
		? "font-black uppercase tracking-wide transition-colors"
		: baseStyles;

	return (
		<button
			className={`${appliedBase} ${variants[variant]} ${sizes[size]} ${className}`}
			{...props}
		>
			{children}
		</button>
	);
}
