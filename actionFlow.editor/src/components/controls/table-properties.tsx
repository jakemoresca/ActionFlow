import { Table, TableBody, TableCell, TableHead, TableHeadCell, TableRow } from "flowbite-react";
import { ChangeEvent } from "react";
import PropertyField from "./property-field";
import { NodePropertyDefinition, NodePropertyType } from "../left-pane/NodeProperties";

export type TablePropertiesData = {
  properties?: Record<string, string>[];
  columnDefinitions: TablePropertiesColumnDefinition[]
  handlePropertyChange?: (event: ChangeEvent) => void
};

export type TablePropertiesColumnDefinition = {
  name: string
  type: NodePropertyType,
  index: number
  field: string
}

export default function TableProperties({ properties, columnDefinitions, handlePropertyChange }: TablePropertiesData) {

  const columnCount = columnDefinitions.length;

  const createTableHeaderCells = () => {
    const headerCells = []

    for (let index = 0; index < columnCount; index++) {
      const columnDefinition = columnDefinitions.find(x => x.index == index);

      if (columnDefinition) {
        headerCells.push(<TableHeadCell key={`properties_headcell_${index}`}>{columnDefinition.name}</TableHeadCell>)
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
      return (<TableRow key={key} className="bg-white dark:border-gray-700 dark:bg-gray-800">{tableCells}</TableRow>)
    });

    return tableRows;
  }

  const createTableCell = (columnDefinition: TablePropertiesColumnDefinition, property: Record<string, string>, rowIndex: number) => {
    const key = `key_${rowIndex}_${property[columnDefinition.field]}`;

    const propertyDefinition: NodePropertyDefinition = {
      propertyName: columnDefinition.field,
      propertyType: columnDefinition.type,
      propertyLabel: columnDefinition.name,
      index: rowIndex
    }

    return (<TableCell key={key}><PropertyField nodeType="" properties={property} propertyDefinition={propertyDefinition} handlePropertyChange={handlePropertyChange} /></TableCell>)
  }

  return (
    <div className="overflow-x-auto mt-3">
      <Table striped>
        <TableHead>
          { createTableHeaderCells() }
        </TableHead>
        <TableBody className="divide-y">
          {createTableRows()}
        </TableBody>
      </Table>
    </div>
  );
}
