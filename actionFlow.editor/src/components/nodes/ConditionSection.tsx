
export type ConditionSectionData = {
    condition?: string
}

export default function ConditionSection({condition}: ConditionSectionData) {
    return (condition ? <div>{condition}</div> : <div></div>)
}