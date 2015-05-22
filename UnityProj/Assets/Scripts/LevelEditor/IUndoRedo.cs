#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public interface IUndo
{
    void Undo();
    void Redo();    
}
#endif

