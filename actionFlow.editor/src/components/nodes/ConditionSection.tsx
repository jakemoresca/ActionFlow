
export type ConditionSectionData = {
    condition?: string
}

export default function ConditionSection({condition}: ConditionSectionData) {
    return (condition ? <p className="text-xs font-normal text-gray-700 dark:text-gray-400">{condition}</p> : <div></div>)
}