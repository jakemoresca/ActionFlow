import { Table, TextInput } from "flowbite-react";
import { ChangeEvent } from "react";

export type TablePropertiesData = {
  properties?: Record<string, string>[];
  columnDefinitions: TablePropertiesColumnDefinition[]
  handlePropertyChange?: (event: ChangeEvent<HTMLInputElement>) => void
};

export type TablePropertiesColumnDefinition = {
  name: string
  type: TablePropertiesColumnType,
  index: number
  field: string
}

export enum TablePropertiesColumnType {
  Label,
  TextField,
  NumberField
}

export default function TableProperties({ properties, columnDefinitions, handlePropertyChange }: TablePropertiesData) {

  const columnCount = columnDefinitions.length;

  const createTableHeaderCells = () => {
    const headerCells = []

    for (let index = 0; index < columnCount; index++) {
      const columnDefinition = columnDefinitions.find(x => x.index == index);

      if (columnDefinition) {
        headerCells.push(<Table.HeadCell key={`properties_headcell_${index}`}>{columnDefinition.name}</Table.HeadCell>)
      }
    }

    return headerCells;
  }

  const createTableRows = () => {
    const tableRows = properties?.map((property, rowIndex) => {
      const tableCells = []
      
      for (let index = 0; index < columnCount; index++) {
        const columnDefinition = columnDefinitions.find(x => x.index == index);
  
        if (columnDefinition) {
          const tableCell = createTableCell(columnDefinition, property, rowIndex)
          tableCells.push(tableCell)
        }
      }

      const key = `row_${rowIndex}`;
      return (<Table.Row key={key} className="bg-white dark:border-gray-700 dark:bg-gray-800">{tableCells}</Table.Row>)
    });

    return tableRows;
  }

  const createTableCell = (columnDefinition: TablePropertiesColumnDefinition, property: Record<string, string>, rowIndex: number) => {
    const id = `row_${rowIndex}_${property[columnDefinition.field]}`;
    const key = `key_${rowIndex}_${property[columnDefinition.field]}`;

    switch(columnDefinition.type) {
      case TablePropertiesColumnType.Label:
        return (<Table.Cell key={key}>{property[columnDefinition.field]}</Table.Cell>)
      case TablePropertiesColumnType.TextField:
        return (<Table.Cell key={key}><TextInput name={id} placeholder={columnDefinition.name} value={property[columnDefinition.field]} onChange={handlePropertyChange} /></Table.Cell>)
    }
  }

  return (
    <div className="overflow-x-auto mt-3">
      <Table striped>
        <Table.Head>
          { createTableHeaderCells() }
        </Table.Head>
        <Table.Body className="divide-y">
          {createTableRows()}
        </Table.Body>
      </Table>
    </div>
  );
}
