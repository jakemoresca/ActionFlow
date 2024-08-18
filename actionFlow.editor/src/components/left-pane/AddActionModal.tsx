import { Modal } from "flowbite-react";
import { NodeTypeKeys } from "../nodes";

export type AddActionModalData = {
  showModal: boolean;
  onCloseModal?: () => void;
  onAddNode?: (nodeType: string) => void;
};

const actionIcon = (
  <svg
    className="w-6 h-6 text-gray-800 dark:text-white"
    aria-hidden="true"
    xmlns="http://www.w3.org/2000/svg"
    width="24"
    height="24"
    fill="currentColor"
    viewBox="0 0 24 24"
  >
    <path
      fillRule="evenodd"
      d="M3 4a1 1 0 0 0-.822 1.57L6.632 12l-4.454 6.43A1 1 0 0 0 3 20h13.153a1 1 0 0 0 .822-.43l4.847-7a1 1 0 0 0 0-1.14l-4.847-7a1 1 0 0 0-.822-.43H3Z"
      clipRule="evenodd"
    />
  </svg>
);

export const AddActionModalDataActions = [
  {
    name: NodeTypeKeys.variable.name,
    type: NodeTypeKeys.variable.type,
    icon: actionIcon,
  },
  {
    name: NodeTypeKeys.sendHttpCall.name,
    type: NodeTypeKeys.sendHttpCall.type,
    icon: actionIcon,
  },
  {
    name: NodeTypeKeys.controlFlow.name,
    type: NodeTypeKeys.controlFlow.type,
    icon: actionIcon,
  },{
    name: NodeTypeKeys.forLoop.name,
    type: NodeTypeKeys.forLoop.type,
    icon: actionIcon,
  },
];

export default function AddActionModal({
  showModal,
  onCloseModal,
  onAddNode
}: AddActionModalData) {
  return (
    <Modal
      show={showModal}
      size="xl"
      onClose={() => onCloseModal && onCloseModal()}
      popup
    >
      <Modal.Header />
      <Modal.Body>
        <h3 className="text-xl font-medium text-gray-900 dark:text-white">
          Add Action
        </h3>
        <div className="grid grid-cols-3 gap-4 p-4 lg:grid-cols-4">
          {AddActionModalDataActions.map((action) => {
            return (
              <div
                key={action.type}
                onClick={() => onAddNode && onAddNode(action.type)}
                className="cursor-pointer rounded-lg bg-gray-50 p-4 hover:bg-gray-100 dark:bg-gray-700 dark:hover:bg-gray-600"
              >
                <div className="mx-auto mb-2 flex h-[48px] max-h-[48px] w-[48px] max-w-[48px] items-center justify-center rounded-full bg-gray-200 p-2 dark:bg-gray-600">
                  {action.icon}
                </div>
                <div className="text-center font-medium text-gray-500 dark:text-gray-400">
                  {action.name}
                </div>
              </div>
            );
          })}
        </div>
      </Modal.Body>
    </Modal>
  );
}
