# DataGridView-StackedHeader
Create Stacked / Multiple Header or Group Header in your DataGridView control

`DataGridView` forms an integral part of any application and with specific needs of the applications, harnessing power of `DataGridView` is essential. I faced a similar scenario in a project where we had a `DataGridView` having more than hundred columns generated dynamically. The columns are generated based on the hierarchy of the inputs (more than twelve different cases each having its own input hierarchy) with the Columns header text depicting the hierarchy, for example, Component End 1 Input1, Component End 1 Input 2, Component End 2 Input 1, Component End 2 Input 2.

Having redundant text in column header is not user friendly requiring the user to read entire text to know what the input is for. We decided to have stacked headers where we group the inputs based on the hierarchy.

![StackedHeader](https://www.codeproject.com/KB/grid/474418/stackedheader1.png)

As we know, the default Winforms DataGridView does not support Stacked Headers and going for a 3rd party Grid just for the Stacked Header was not of much value so I had to do this on my own.

The solution is divided into four steps:

1. Since I already had the column header text generated based on the hierarchy, I decided to use header text for grouping. For this, I changed the header text to use ‘.’ to define hierarchy. E.g.: Component.End 1.Input1, Component.End 1.Input 2, Component.End 2.Input 1, Component.End 2.Input 2
2. Generate a simple tree representing hierarchy of the columns.
3. Measure width required by each group (considering columns width, visibility).
4. Render the headers.

As a result of this exercise, a developer can quickly convert the column headers to stacked header by changing the header text and using this one line of code to draw stacked headers.

As a result of this exercise, a developer can quickly convert the column headers to stacked header by changing the header text and using this one line of code to draw stacked headers.

```csharp
StackedHeaderDecorator objRenderer = new StackedHeaderDecorator(objDataGrid);
```

This one line takes care of step 2, 3 and 4 of the DataGridView while leaving Step 1 to the user of this solution.

## Code

![StackedHeader](https://www.codeproject.com/KB/grid/474418/ClassDiagram.png)

The component consists of three classes and an interface.

### Header

Represents a header and its children. As a whole, it forms the representation of the headers as a tree which is rendered by `StackedHeaderDecorator`.

#### Properties

- `Children`: Holds the children rendered under this header
- `Name`: Name of the header, used by the renderer as the header text to be drawn.
- `X` and `Y`: Left, Top location of the start of the header.
- `Width` and `Height`: Size of the region taken by this header. This is set dynamically when the measuring of the header is done.
- `ColumnId`: If this is a lowest header, it is the id of the column it represents, else it is the id of the first visible column in the Header in `Children` property.

#### Methods

##### AcceptRenderer

Accepts the renderer which renders this header. It first paints the children, then self.

```csharp
public void AcceptRenderer(StackedHeaderDecorator objRenderer)
{
    foreach (Header objChild in Children)
    {
        objChild.AcceptRenderer(objRenderer);
    }
    if (-1 != ColumnId && !string.IsNullOrEmpty(Name.Trim()))
    {
        objRenderer.Render(this);
    }
}
```

##### Measure

Calculates the region required by the `Header` including its `Children`.

```csharp
public void Measure(DataGridView objGrid, int iY, int iHeight)
{
    Width = 0;
    if (Children.Count > 0)
    {
        int tempY = string.IsNullOrEmpty(Name.Trim()) ? iY : iY + iHeight;
        bool columnWidthSet = false;
        foreach (Header child in Children)
        {
            child.Measure(objGrid, tempY, iHeight);
            Width += child.Width;
            if (!columnWidthSet && Width > 0)
            {
                ColumnId = child.ColumnId;
                columnWidthSet = true;
            }
        }
    }
    else if (-1 != ColumnId && objGrid.Columns[ColumnId].Visible)
    {
        Width = objGrid.Columns[ColumnId].Width;
    }
    Y = iY;
    if (Children.Count == 0)
    {
        Height = objGrid.ColumnHeadersHeight - iY;
    }
    else
    {
        Height = iHeight;
    }
}
```

#### StackedHeaderDecorator

Decorates the `DataGridView` hooking into it several events which paint/refresh the header of the DataGridView. It also enables `DoubleBuffering` on the `DataGrid`.

It uses an instance of implementation of `IStackedHeaderGenerator` to generate the headers. By default, it uses the `StackedHeaderGenerator` implementation of `IStackedHeaderGenerator` which uses the `HeaderText` to generate the Header tree. You can pass your implementation of the Generator via the overloaded constructor.

These are the events handlers hooked to the `DataGridView`.

```csharp
objDataGrid.Scroll += objDataGrid_Scroll;
objDataGrid.Paint += objDataGrid_Paint;
objDataGrid.ColumnRemoved += objDataGrid_ColumnRemoved;
objDataGrid.ColumnAdded += objDataGrid_ColumnAdded;
objDataGrid.ColumnWidthChanged += objDataGrid_ColumnWidthChanged;
```

All events other than `PaintEvent` just invalidate the `DataGridView` regions, so we will look into the `PaintEvent` `Handler`, `RenderColumnHeaders` and `Render` which do the heavy lifting.

##### PaintEvent Handler

Calculates the number of levels of stacking, sets the height of the `ColumnHeader` and calls `RenderColumnHeaders`.

```csharp
void objDataGrid_Paint(object sender, PaintEventArgs e)
{
    iNoOfLevels = NoOfLevels(objHeaderTree);
    objGraphics = e.Graphics;
    objDataGrid.ColumnHeadersHeightSizeMode =
             DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
    objDataGrid.ColumnHeadersHeight = iNoOfLevels * 20;
    if (null != objHeaderTree)
    {
        RenderColumnHeaders();
    }
}
```

##### RenderColumnHeaders

Fills the background rectangle of the column header, then loops through each child measuring and rendering.

```csharp
private void RenderColumnHeaders()
{
    objGraphics.FillRectangle(new SolidBrush(objDataGrid.ColumnHeadersDefaultCellStyle.BackColor),
                              new Rectangle(objDataGrid.DisplayRectangle.X,
                                            objDataGrid.DisplayRectangle.Y,
                                            objDataGrid.DisplayRectangle.Width,
                                            objDataGrid.ColumnHeadersHeight));

    foreach (Header objChild in objHeaderTree.Children)
    {
        objChild.Measure(objDataGrid, 0, objDataGrid.ColumnHeadersHeight/iNoOfLevels);
        objChild.AcceptRenderer(this);
    }
}
```

##### RenderColumnHeaders

Renders the `header`, it checks if it is a leaf header or parent header. It uses the Clip feature of GDI+ to correctly draw the clipped header when user scrolls the `DataGridView`.

```csharp
public void Render(Header objHeader)
{
    if (objHeader.Children.Count == 0)
    {
        Rectangle r1 = objDataGrid.GetColumnDisplayRectangle(objHeader.ColumnId, true);
        if (r1.Width == 0)
        {
            return;
        }
        r1.Y = objHeader.Y;
        r1.Width += 1;
        r1.X -= 1;
        r1.Height = objHeader.Height;
        objGraphics.SetClip(r1);

        if (r1.X + objDataGrid.Columns
            [objHeader.ColumnId].Width < objDataGrid.DisplayRectangle.Width)
        {
            r1.X -= (objDataGrid.Columns[objHeader.ColumnId].Width - r1.Width);
        }
        r1.X -= 1;
        r1.Width = objDataGrid.Columns[objHeader.ColumnId].Width;
        objGraphics.DrawRectangle(Pens.Gray, r1);
        objGraphics.DrawString(objHeader.Name,
                                objDataGrid.ColumnHeadersDefaultCellStyle.Font,
                                new SolidBrush
                                (objDataGrid.ColumnHeadersDefaultCellStyle.ForeColor),
                                r1,
                                objFormat);
        objGraphics.ResetClip();
    }
    else
    {
        int x = objDataGrid.RowHeadersWidth;
        for (int i = 0; i < objHeader.Children[0].ColumnId; ++i)
        {
            if (objDataGrid.Columns[i].Visible)
            {
                x += objDataGrid.Columns[i].Width;
            }
        }
        if (x > (objDataGrid.HorizontalScrollingOffset + objDataGrid.DisplayRectangle.Width - 5))
        {
            return;
        }

        Rectangle r1 = objDataGrid.GetCellDisplayRectangle(objHeader.ColumnId, -1, true);
        r1.Y = objHeader.Y;
        r1.Height = objHeader.Height;
        r1.Width = objHeader.Width  + 1;
        if (r1.X < objDataGrid.RowHeadersWidth)
        {
            r1.X = objDataGrid.RowHeadersWidth;
        }
        r1.X -= 1;
        objGraphics.SetClip(r1);
        r1.X = x - objDataGrid.HorizontalScrollingOffset;
        r1.Width -= 1;
        objGraphics.DrawRectangle(Pens.Gray, r1);
        r1.X -= 1;
        objGraphics.DrawString(objHeader.Name, objDataGrid.ColumnHeadersDefaultCellStyle.Font,
                                new SolidBrush(objDataGrid.ColumnHeadersDefaultCellStyle.ForeColor),
                                r1, objFormat);
        objGraphics.ResetClip();
    }
}
```

There is one bug and several improvements I see in this code and I will work on them and update the articles.

##### Update

Have written another class `"DataGridExporter"` which exports the data from the DataGridView to HTML table, which you can save as Excel, including the header grouping. The updated source code is attached with this article while you can find more details on it [here](https://www.codeproject.com/Articles/484948/DataGridViewplus-e2-80-93plusStackedplusHeaderplus).

##### Bug

If you plan to use the default `StackedHeaderGenerator` for header generation, then you have to know there is a bug in there which crops up if you have two top level headers with the same name but do not represent consecutive columns, then the header generated is incorrect. Since it uses '.' separated header text to generate headers, you can add additional '.'s to get the correct header tree.

**Note**: The source code posted is not completely tested and polished, please use it diligently.

This article was originally posted at [http://deepakvs.wordpress.com/2012/10/09/datagridview-stacked-header](http://deepakvs.wordpress.com/2012/10/09/datagridview-stacked-header)

# License
This article, along with any associated source code and files, is licensed under [The Code Project Open License (CPOL)](https://www.codeproject.com/info/cpol10.aspx)

Written By
[Deepak-VS](https://www.codeproject.com/Members/Deepak-VS)
India

-----
Reference
- [DataGridView – Stacked Header](https://www.codeproject.com/Articles/474418/DataGridViewplus-e-plusStackedplusHeader)
- [DataGridView - DualHeader](https://github.com/marcalfaro/DataGridView---DualHeader)
- [DGVGroupHeaders](https://github.com/marcalfaro/DGVGroupHeaders)