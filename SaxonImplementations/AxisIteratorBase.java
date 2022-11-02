import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.tree.iter.AxisIterator;

public abstract class AxisIteratorBase implements AxisIterator
{

    @Override
    public abstract NodeInfo next();

}