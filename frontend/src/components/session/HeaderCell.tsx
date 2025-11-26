export type SortKey = "name" | "status" | "time";

export interface SortConfig {
	key: SortKey;
	direction: "asc" | "desc";
}

interface HeaderCellProps {
	label: string;
	sortKey: SortKey;
	currentSort: SortConfig;
	onSort: (key: SortKey) => void;
	className?: string;
}

export function HeaderCell({
	label,
	sortKey,
	currentSort,
	onSort,
	className = "",
}: HeaderCellProps) {
	const isActive = currentSort.key === sortKey;

	return (
		<button
			type="button"
			onClick={() => onSort(sortKey)}
			className={`${className} cursor-pointer select-none bg-grey-400 gap-2 px-2 py-1 flex items-center`}
		>
			{label}
			{isActive ? (
				<span
					className={`text-xs text-black px-1 ${currentSort.direction === "desc" ? "rotate-180" : ""}`}
				>
					▼
				</span>
			) : (
				<span className="text-xs opacity-0">▼</span>
			)}
		</button>
	);
}
