import { type ReactNode, useEffect, useRef } from "react";
import { BlockyButton } from "./BlockyButton";
import { BlockyCard } from "./BlockyCard";

interface BlockyModalProps {
	isOpen: boolean;
	onClose: () => void;
	title?: string;
	children: ReactNode;
}

export function BlockyModal({
	isOpen,
	onClose,
	title,
	children,
}: BlockyModalProps) {
	const dialogRef = useRef<HTMLDialogElement>(null);

	useEffect(() => {
		const dialog = dialogRef.current;
		if (!dialog) return;

		if (isOpen) {
			if (!dialog.open) {
				dialog.showModal();
			}
		} else {
			if (dialog.open) {
				dialog.close();
			}
		}
	}, [isOpen]);

	const handleBackdropClick = (e: React.MouseEvent<HTMLDialogElement>) => {
		if (e.target === dialogRef.current) {
			onClose();
		}
	};

	return (
		// biome-ignore lint/a11y/useKeyWithClickEvents: Native dialog handles keyboard (Esc) automatically. click is for backdrop only.
		<dialog
			ref={dialogRef}
			className="bg-transparent p-0 m-auto backdrop:bg-black/50 backdrop:backdrop-blur-sm open:animate-in open:fade-in open:zoom-in-95 duration-200"
			onClick={handleBackdropClick}
			onClose={onClose}
		>
			<div className="w-full max-w-md p-4">
				<BlockyCard className="bg-white shadow-[8px_8px_0px_0px_rgba(0,0,0,1)] p-6">
					<div className="flex justify-between items-start mb-6">
						{title && (
							<h2 className="text-2xl font-black uppercase">{title}</h2>
						)}
						<BlockyButton
							variant="danger"
							size="sm"
							onClick={onClose}
							className="ml-4 px-3"
							aria-label="Close modal"
						>
							X
						</BlockyButton>
					</div>
					{children}
				</BlockyCard>
			</div>
		</dialog>
	);
}
