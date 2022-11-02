////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 Martin Honnen
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// adapted from https://saxonica.plan.io/projects/saxonmirrorhe/repository/he/revisions/he-saxon10_6/entry/src/main/java/net/sf/saxon/dotnet/DotNetNodeWrapper.java
// which is
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using net.sf.saxon.s9api;

using net.sf.saxon.om;

using NamespaceConstant = net.sf.saxon.lib.NamespaceConstant;

using NamespaceReducer = net.sf.saxon.@event.NamespaceReducer;
using Receiver = net.sf.saxon.@event.Receiver;

using AnyNodeTest = net.sf.saxon.pattern.AnyNodeTest;
using NameTest = net.sf.saxon.pattern.NameTest;
using NodeTest = net.sf.saxon.pattern.NodeTest;

using XPathException = net.sf.saxon.trans.XPathException;

using AbstractNodeWrapper = net.sf.saxon.tree.wrapper.AbstractNodeWrapper;
using SiblingCountingNode = net.sf.saxon.tree.wrapper.SiblingCountingNode;

using SequenceIterator = net.sf.saxon.om.SequenceIterator;

using AxisIterator = net.sf.saxon.tree.iter.AxisIterator;
using LookaheadIterator = net.sf.saxon.tree.iter.LookaheadIterator;

//using FastStringBuffer = net.sf.saxon.tree.util.FastStringBuffer;

using Navigator = net.sf.saxon.tree.util.Navigator;
using SteppingNavigator = net.sf.saxon.tree.util.SteppingNavigator;
using SteppingNode = net.sf.saxon.tree.util.SteppingNode;

using SaxonType = net.sf.saxon.type.Type;

//using Property = net.sf.saxon.om.SequenceIterator.Property;

using JArrayList = java.util.ArrayList;
using JEnumSet = java.util.EnumSet;
using JHashSet = java.util.HashSet;
using JSet = java.util.Set;
using JPredicate = java.util.function.Predicate;
using net.sf.saxon.str;
using java.lang;
using net.sf.saxon.tree.iter;

namespace SaxonHE11s9apiExtensions
{
    public class DotNetNodeWrapper : AbstractNodeWrapper, SteppingNodeBase, SiblingCountingNode
    {
        protected internal XmlNode node;
        protected short nodeKind;
        protected DotNetNodeWrapper parent;     // null means unknown
        protected DotNetDocumentWrapper docWrapper;
        protected int index;            // -1 means unknown
        protected int span = 1;         // the number of adjacent text nodes wrapped by this NodeWrapper.
                                        // If span>1, node will always be the first of a sequence of adjacent text nodes

        /**
         * This constructor is protected: nodes should be created using the makeWrapper
         * factory method
         *
         * @param node   The DOM node to be wrapped
         * @param parent The NodeWrapper that wraps the parent of this node
         * @param index  Position of this node among its siblings
         */
        protected DotNetNodeWrapper(XmlNode node, DotNetNodeWrapper parent, int index)
        {
            //System.err.println("Creating NodeWrapper for " +node);
            this.node = node;
            this.parent = parent;
            this.index = index;
        }

        DotNetNodeWrapper()
        {
        }

        /**
         * Factory method to wrap a DOM node with a wrapper that implements the Saxon
         * NodeInfo interface.
         *
         * @param node       The DOM node
         * @param docWrapper The wrapper for the containing Document node
         * @return The new wrapper for the supplied node
         * @throws NullPointerException if the node or the document wrapper are null
         */
        protected internal static DotNetNodeWrapper makeWrapper(XmlNode node, DotNetDocumentWrapper docWrapper)
        {
            if (node == null)
            {
                throw new NullReferenceException("NodeWrapper#makeWrapper: Node must not be null");
            }
            if (docWrapper == null)
            {
                throw new NullReferenceException("NodeWrapper#makeWrapper: DocumentWrapper must not be null");
            }
            return makeWrapper(node, docWrapper, null, -1);
        }

        /**
         * Factory method to wrap a DOM node with a wrapper that implements the Saxon
         * NodeInfo interface.
         *
         * @param node       The DOM node
         * @param docWrapper The wrapper for the containing Document node     *
         * @param parent     The wrapper for the parent of the JDOM node
         * @param index      The position of this node relative to its siblings
         * @return The new wrapper for the supplied node
         */

        protected static DotNetNodeWrapper makeWrapper(XmlNode node, DotNetDocumentWrapper docWrapper,
                                                /*@Nullable*/ DotNetNodeWrapper parent, int index)
        {
            DotNetNodeWrapper wrapper;
            switch (node.NodeType)
            {
                case XmlNodeType.Document:
                    //case Node.DOCUMENT_FRAGMENT_NODE:
                    wrapper = (DotNetNodeWrapper)docWrapper.getRootNode();
                    if (wrapper == null)
                    {
                        wrapper = new DotNetNodeWrapper(node, parent, index);
                        wrapper.nodeKind = SaxonType.DOCUMENT;
                    }
                    break;
                case XmlNodeType.Element:
                    wrapper = new DotNetNodeWrapper(node, parent, index);
                    wrapper.nodeKind = SaxonType.ELEMENT;
                    break;
                case XmlNodeType.Attribute:
                    wrapper = new DotNetNodeWrapper(node, parent, index);
                    wrapper.nodeKind = SaxonType.ATTRIBUTE;
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    wrapper = new DotNetNodeWrapper(node, parent, index);
                    wrapper.nodeKind = SaxonType.TEXT;
                    break;

                case XmlNodeType.Comment:
                    wrapper = new DotNetNodeWrapper(node, parent, index);
                    wrapper.nodeKind = SaxonType.COMMENT;
                    break;
                case XmlNodeType.ProcessingInstruction:
                    wrapper = new DotNetNodeWrapper(node, parent, index);
                    wrapper.nodeKind = SaxonType.PROCESSING_INSTRUCTION;
                    break;
                default:
                    throw new IllegalArgumentException("Unsupported node type in DOM! " + node.NodeType + " instance " + node.ToString());
            }
            wrapper.docWrapper = docWrapper;
            wrapper.treeInfo = docWrapper;
            return wrapper;
        }

        /**
         * Get the underlying DOM node, to implement the VirtualNode interface
         */

        // @Override
        public override XmlNode getUnderlyingNode()
        {
            return node;
        }

        /**
         * Return the type of node.
         *
         * @return one of the values Node.ELEMENT, Node.TEXT, Node.ATTRIBUTE, etc.
         */

        // @Override
        public override int getNodeKind()
        {
            return nodeKind;
        }


        /**
         * Determine whether this is the same node as another node.
         * <p>Note: a.isSameNodeInfo(b) if and only if generateId(a)==generateId(b)</p>
         *
         * @return true if this Node object and the supplied Node object represent the
         *         same node in the tree.
         */

        public bool equals(NodeInfo other)
        {
            // On .NET, the DOM appears to guarantee that the same node is always represented
            // by the same object

            return other is DotNetNodeWrapper && node == ((DotNetNodeWrapper)other).node;

        }

        /**
         * The hashCode() method obeys the contract for hashCode(): that is, if two objects are equal
         * (represent the same node) then they must have the same hashCode()
         *
         * @since 8.7 Previously, the effect of the equals() and hashCode() methods was not defined. Callers
         *        should therefore be aware that third party implementations of the NodeInfo interface may
         *        not implement the correct semantics.
         */

        public override int hashCode()
        {
            StringBuilder buffer = new StringBuilder();//FastStringBuffer buffer = new FastStringBuffer(FastStringBuffer.C64);
            generateId(buffer);
            return buffer.toString().GetHashCode();
        }

        /**
         * Determine the relative position of this node and another node, in document order.
         * The other node will always be in the same document.
         *
         * @param other The other node, whose position is to be compared with this node
         * @return -1 if this node precedes the other node, +1 if it follows the other
         *         node, or 0 if they are the same node. (In this case, isSameNode() will always
         *         return true, and the two nodes will produce the same result for generateId())
         */

        // @Override
        public override int compareOrder(NodeInfo other)
        {
            if (other is SiblingCountingNode)
            {
                return Navigator.compareOrder(this, (SiblingCountingNode)other);
            }
            else
            {
                // it's presumably a Namespace Node
                return -other.compareOrder(this);
            }
        }

        ///**
        // * Get the value of the item as a CharSequence. This is in some cases more efficient than
        // * the version of the method that returns a String.
        // */

        //// @Override
        //public override string getStringValue()
        //{
        //    return base.getStringValue();
        //}
        public override UnicodeString getUnicodeStringValue()
        {
            switch (nodeKind)
            {
                case SaxonType.DOCUMENT:
                case SaxonType.ELEMENT:
                    return StringView.of(node.InnerText);

                case SaxonType.ATTRIBUTE:
                    return StringView.of(node.Value);

                case SaxonType.TEXT:
                    if (span == 1)
                    {
                        return StringView.of(node.InnerText);
                    }
                    else
                    {
                        StringBuffer sb = new StringBuffer();//FastStringBuffer(FastStringBuffer.C64);
                        XmlNode textNode = node;
                        for (int i = 0; i < span; i++)
                        {
                            sb.append(textNode.InnerText);
                            textNode = textNode.NextSibling;
                        }
                        return StringView.of(sb.toString());
                    }

                case SaxonType.COMMENT:
                case SaxonType.PROCESSING_INSTRUCTION:
                    return StringView.of(node.Value);

                default:
                    return EmptyUnicodeString.getInstance();
            }
        }

        /**
         * Get the local part of the name of this node. This is the name after the ":" if any.
         *
         * @return the local part of the name. For an unnamed node, returns null, except for
         *         un unnamed namespace node, which returns "".
         */

        // @Override
        public override string getLocalPart()
        {
            return node.LocalName;
        }

        /**
         * Get the URI part of the name of this node. This is the URI corresponding to the
         * prefix, or the URI of the default namespace if appropriate.
         *
         * @return The URI of the namespace of this node. For an unnamed node,
         *         or for a node with an empty prefix, return an empty
         *         string.
         */

        // @Override
        public override string getURI()
        {
            NodeInfo element;
            if (nodeKind == SaxonType.ELEMENT)
            {
                element = this;
            }
            else if (nodeKind == SaxonType.ATTRIBUTE)
            {
                element = parent;
            }
            else
            {
                return "";
            }

            // The DOM methods getPrefix() and getNamespaceURI() do not always
            // return the prefix and the URI; they both return null, unless the
            // prefix and URI have been explicitly set in the node by using DOM
            // level 2 interfaces. There's no obvious way of deciding whether
            // an element whose name has no prefix is in the default namespace,
            // other than searching for a default namespace declaration. So we have to
            // be prepared to search.

            // If getPrefix() and getNamespaceURI() are non-null, however,
            // we can use the values.

            string uri = node.NamespaceURI;
            if (uri != null)
            {
                return uri;
            }

            // Otherwise we have to work it out the hard way...

            if (node.Name.StartsWith("xml:"))
            {
                return NamespaceConstant.XML;
            }

            string[] parts;
            try
            {
                parts = NameChecker.getQNameParts(node.Name);
            }
            catch (QNameException e)
            {
                throw new IllegalStateException("Invalid QName in DOM node. " + e);
            }

            if (nodeKind == SaxonType.ATTRIBUTE && parts[0] == string.Empty)
            {
                // for an attribute, no prefix means no namespace
                uri = "";
            }
            else
            {
                AxisIterator nsiter = element.iterateAxis(AxisInfo.NAMESPACE);
                NodeInfo ns;
                while ((ns = nsiter.next()) != null)
                {
                    if (ns.getLocalPart() == parts[0])
                    {
                        uri = ns.getStringValue();
                        break;
                    }
                }
                if (uri == null)
                {
                    if (parts[0] == string.Empty)
                    {
                        uri = "";
                    }
                    else
                    {
                        throw new IllegalStateException("Undeclared namespace prefix in DOM input: " + parts[0]);
                    }
                }
            }
            return uri;
        }

        /**
         * Get the prefix of the name of the node. This is defined only for elements and attributes.
         * If the node has no prefix, or for other kinds of node, return a zero-length string.
         * This implementation simply returns the prefix defined in the DOM model; this is not strictly
         * accurate in all cases, but is good enough for the purpose.
         *
         * @return The prefix of the name of the node.
         */

        // @Override
        public override string getPrefix()
        {
            return node.Prefix;
        }

        /**
         * Get the display name of this node. For elements and attributes this is [prefix:]localname.
         * For unnamed nodes, it is an empty string.
         *
         * @return The display name of this node.
         *         For a node with no name, return an empty string.
         */

        // @Override
        public override string getDisplayName()
        {
            switch (nodeKind)
            {
                case SaxonType.ELEMENT:
                case SaxonType.ATTRIBUTE:
                case SaxonType.PROCESSING_INSTRUCTION:
                    return node.Name;
                default:
                    return "";

            }
        }

        /**
         * Get the NodeInfo object representing the parent of this node
         */

        // @Override
        public override NodeInfo getParent()
        {
            if (parent == null)
            {
                switch (getNodeKind())
                {
                    case SaxonType.ATTRIBUTE:
                        parent = makeWrapper(((XmlAttribute)node).OwnerElement, docWrapper);
                        return parent;
                    default:
                        XmlNode p = node.ParentNode;
                        if (p == null)
                        {
                            return null;
                        }
                        else
                        {
                            parent = makeWrapper(p, docWrapper);
                            return parent;
                        }
                }
            }
            return parent;
        }

        /**
         * Get the index position of this node among its siblings (starting from 0).
         * In the case of a text node that maps to several adjacent siblings in the DOM,
         * the numbering actually refers to the position of the underlying DOM nodes;
         * thus the sibling position for the text node is that of the first DOM node
         * to which it relates, and the numbering of subsequent XPath nodes is not necessarily
         * consecutive.
         */

        // @Override
        public int getSiblingPosition()
        {
            if (index == -1)
            {
                switch (nodeKind)
                {
                    case SaxonType.ELEMENT:
                    case SaxonType.TEXT:
                    case SaxonType.COMMENT:
                    case SaxonType.PROCESSING_INSTRUCTION:
                        int ix = 0;
                        XmlNode start = node;
                        while (true)
                        {
                            start = start.PreviousSibling;
                            if (start == null)
                            {
                                index = ix;
                                return ix;
                            }
                            ix++;
                        }
                    case SaxonType.ATTRIBUTE:
                        ix = 0;
                        AxisIterator iter = parent.iterateAxis(AxisInfo.ATTRIBUTE);
                        while (true)
                        {
                            NodeInfo n = iter.next();
                            if (n == null || Navigator.haveSameName(this, n))
                            {
                                index = ix;
                                return ix;
                            }
                            ix++;
                        }

                    case SaxonType.NAMESPACE:
                        ix = 0;
                        iter = parent.iterateAxis(AxisInfo.NAMESPACE);
                        while (true)
                        {
                            NodeInfo n = iter.next();
                            if (n == null || Navigator.haveSameName(this, n))
                            {
                                index = ix;
                                return ix;
                            }
                            ix++;
                        }
                    default:
                        index = 0;
                        return index;
                }
            }
            return index;
        }

        // @Override
        protected override AxisIterator iterateAttributes(NodeTest nodeTest) //(JPredicate nodeTest) //Predicate<? super NodeInfo> nodeTest)
        {
            AxisIterator iter = new AttributeEnumeration(this.docWrapper, this);
            if (nodeTest != AnyNodeTest.getInstance())
            {
                iter = new Navigator.AxisFilter(iter, (net.sf.saxon.pattern.NodePredicate)nodeTest);
            }
            return iter;
        }

        // @Override
        protected override AxisIterator iterateChildren(NodeTest nodeTest)  //(Predicate<? super NodeInfo> nodeTest)
        {
            AxisIterator iter = new ChildEnumeration(this, true, true);
            if (nodeTest != AnyNodeTest.getInstance())
            {
                iter = new Navigator.AxisFilter(iter, (net.sf.saxon.pattern.NodePredicate)nodeTest);
            }
            return iter;
        }

        // @Override
        protected override AxisIterator iterateSiblings(NodeTest nodeTest, bool forwards) //(Predicate<? super NodeInfo> nodeTest, bool forwards)
        {
            AxisIterator iter = new ChildEnumeration(this, false, forwards);
            if (nodeTest != AnyNodeTest.getInstance())
            {
                iter = new Navigator.AxisFilter(iter, (net.sf.saxon.pattern.NodePredicate)nodeTest);
            }
            return iter;
        }

        // @Override
        protected override AxisIterator iterateDescendants(NodeTest nodeTest, bool includeSelf) // (Predicate<? super NodeInfo> nodeTest, bool includeSelf)
        {
            return new SteppingNavigator.DescendantAxisIterator(this, includeSelf, nodeTest);
        }


        // @Override
        public override NamespaceMap getAllNamespaces()
        {
            // Note: in a DOM created by the XML parser, all namespaces are present as attribute nodes. But
            // in a DOM created programmatically, this is not necessarily the case. So we need to add
            // namespace bindings for the namespace of the element and any attributes
            if (node.NodeType == XmlNodeType.Element)
            {
                XmlElement elem = (XmlElement)node;
                XmlNamedNodeMap atts = elem.Attributes;

                NamespaceMap codes = NamespaceMap.emptyMap();
                for (int i = 0; i < atts.Count; i++)
                {
                    XmlAttribute att = (XmlAttribute)atts.Item(i);
                    string attName = att.Name;
                    if (attName == "xmlns")
                    {
                        string prefix = "";
                        string uri = att.Value;
                        codes.put(prefix, uri);
                    }
                    else if (attName.StartsWith("xmlns:"))
                    {
                        string prefix = attName.Substring(6);
                        string uri = att.Value;
                        codes.put(prefix, uri);
                    }
                    else if (att.NamespaceURI.Length != 0)
                    {
                        codes.put(att.Prefix, att.NamespaceURI);
                    }
                }

                if (elem.NamespaceURI.Length != 0)
                {
                    codes.put(elem.Prefix, elem.NamespaceURI);
                }
                /*int count = codes.size();
                NamespaceMap result = new NamespaceBinding[count];

                int p = 0;
                for (NamespaceBinding code : codes) {
                    result[p++] = code;
                } */
                return codes;
            }
            else
            {
                return null;
            }

        }

        /**
         * Get the string value of a given attribute of this node
         *
         * @param uri   the namespace URI of the attribute name. Supply the empty string for an attribute
         *              that is in no namespace
         * @param local the local part of the attribute name.
         * @return the attribute value if it exists, or null if it does not exist. Always returns null
         *         if this node is not an element.
         * @since 9.4
         */
        // @Override
        public override string getAttributeValue(/*@NotNull*/ string uri, /*@NotNull*/ string local)
        {
            NameTest test = new NameTest(SaxonType.ATTRIBUTE, uri, local, getNamePool());
            AxisIterator iterator = iterateAxis(AxisInfo.ATTRIBUTE, test);
            NodeInfo attribute = iterator.next();
            if (attribute == null)
            {
                return null;
            }
            else
            {
                return attribute.getStringValue();
            }
        }

        /**
         * Determine whether the node has any children.
         * <p>Note: the result is equivalent to
         * <code>getEnumeration(Axis.CHILD, AnyNodeTest.getInstance()).hasNext()</code></p>
         */

        // @Override
        public override bool hasChildNodes()
        {
            return node.NodeType != XmlNodeType.Attribute &&
                    node.HasChildNodes;
        }

        /**
         * Get a character string that uniquely identifies this node.
         * Note: a.isSameNode(b) if and only if generateId(a)==generateId(b)
         *
         * @param buffer a buffer to contain a string that uniquely identifies this node, across all
         *               documents
         */

        // @Override
        public override void generateId(StringBuilder buffer)
        {
            Navigator.appendSequentialKey(this, buffer, true);
        }

        /**
         * Copy this node to a given outputter (deep copy)
         */

        //    // @Override
        public override void copy(Receiver @out, int copyOptions, Location locationId) //throws XPathException
        {
            Receiver r = new NamespaceReducer(@out);
            Navigator.copy(this, r, copyOptions, locationId);
        }

        /**
         * Get all namespace undeclarations and undeclarations defined on this element.
         *
         * @param buffer If this is non-null, and the result array fits in this buffer, then the result
         *               may overwrite the contents of this array, to avoid the cost of allocating a new array on the heap.
         * @return An array of integers representing the namespace declarations and undeclarations present on
         *         this element. For a node other than an element, return null. Otherwise, the returned array is a
         *         sequence of namespace codes, whose meaning may be interpreted by reference to the name pool. The
         *         top half word of each namespace code represents the prefix, the bottom half represents the URI.
         *         If the bottom half is zero, then this is a namespace undeclaration rather than a declaration.
         *         The XML namespace is never included in the list. If the supplied array is larger than required,
         *         then the first unused entry will be set to -1.
         *         <p>For a node other than an element, the method returns null.</p>
         */

        // @Override
        public override NamespaceBinding[] getDeclaredNamespaces(NamespaceBinding[] buffer)
        {
            // Note: in a DOM created by the XML parser, all namespaces are present as attribute nodes. But
            // in a DOM created programmatically, this is not necessarily the case. So we need to add
            // namespace bindings for the namespace of the element and any attributes
            if (node.NodeType == XmlNodeType.Element)
            {
                XmlElement elem = (XmlElement)node;
                XmlNamedNodeMap atts = elem.Attributes;
                HashSet<NamespaceBinding> codes = new HashSet<NamespaceBinding>();//JSet codes = new JHashSet(); //Set<NamespaceBinding> codes = new HashSet<NamespaceBinding>();
                for (int i = 0; i < atts.Count; i++)
                {
                    XmlAttribute att = (XmlAttribute)atts.Item(i);
                    string attName = att.Name;
                    if (attName == "xmlns")
                    {
                        string prefix = "";
                        string uri = att.Value;
                        codes.Add(new NamespaceBinding(prefix, uri));
                    }
                    else if (attName.StartsWith("xmlns:"))
                    {
                        string prefix = attName.Substring(6);
                        string uri = att.Value;
                        codes.Add(new NamespaceBinding(prefix, uri));
                    }
                    else if (att.NamespaceURI.Length != 0)
                    {
                        codes.Add(new NamespaceBinding(att.Prefix, att.NamespaceURI));
                    }
                }

                if (elem.NamespaceURI.Length != 0)
                {
                    codes.Add(new NamespaceBinding(elem.Prefix, elem.NamespaceURI));
                }
                //int count = codes.size();
                NamespaceBinding[] result = codes.ToArray();
                //int p = 0;
                //foreach (NamespaceBinding code in codes)
                //for (int p = 0; p < codes.size(); p++)
                //{
                //    result[p] = codes.;
                //}
                return result;
            }
            else
            {
                return null;
            }
        }

        // @Override
        public SteppingNode getNextSibling()
        {
            XmlNode currNode = node.NextSibling;
            if (currNode != null)
            {
                return makeWrapper(currNode, docWrapper);
            }
            return null;
        }


        // @Override
        public SteppingNode getFirstChild()
        {
            XmlNode currNode = node.FirstChild;
            if (currNode != null)
            {
                return makeWrapper(currNode, docWrapper);
            }
            return null;
        }

        // @Override
        public SteppingNode getPreviousSibling()
        {
            XmlNode currNode = node.PreviousSibling;
            if (currNode != null)
            {
                return makeWrapper(currNode, docWrapper);
            }
            return null;
        }

        // @Override
        public SteppingNode getSuccessorElement(SteppingNode anchor, string uri, string local)
        {
            XmlNode stop = anchor == null ? null : ((DotNetNodeWrapper)anchor).node;
            XmlNode next = node;
            do
            {
                next = getSuccessorNode(next, stop);
            } while (next != null &&
                    !(next.NodeType == XmlNodeType.Element &&
                            (uri == null || uri == next.NamespaceURI)) &&
                            (local == null || local == next.LocalName));
            if (next == null)
            {
                return null;
            }
            else
            {
                return makeWrapper(next, docWrapper);
            }
        }

        /**
         * Get the following DOM node in an iteration of a subtree
         *
         * @param start  the start DOM node
         * @param anchor the DOM node marking the root of the subtree within which navigation takes place (may be null)
         * @return the next DOM node in document order after the start node, excluding attributes and namespaces
         */

        private static XmlNode getSuccessorNode(XmlNode start, XmlNode anchor)
        {
            if (start.HasChildNodes)
            {
                return start.FirstChild;
            }
            if (anchor != null && start == anchor)
            {
                return null;
            }
            XmlNode p = start;
            while (true)
            {
                XmlNode s = p.NextSibling;
                if (s != null)
                {
                    return s;
                }
                p = p.ParentNode;
                if (p == null || (anchor != null && p == anchor))
                {
                    return null;
                }
            }
        }

        SteppingNode getParent()
        {
            return ((SteppingNode)parent).getParent();
        }

        //SteppingNode SteppingNode.getNextSibling()
        //{
        //    return ((SteppingNode)parent).getNextSibling();
        //}

        //SteppingNode SteppingNode.getPreviousSibling()
        //{
        //    return ((SteppingNode)parent).getPreviousSibling();
        //}

        //SteppingNode SteppingNode.getFirstChild()
        //{
        //    return ((SteppingNode)parent).getFirstChild();
        //}

        //SteppingNode SteppingNode.getSuccessorElement(SteppingNode sn, string str1, string str2)
        //{
        //    return ((SteppingNode)parent).getSuccessorElement(sn, str1, str2);
        //}

        //public NodeInfo <bridge>getParent()
        //{
        //    return ((SteppingNode)parent).<bridge>getParent();
        //}

        sealed class AttributeEnumeration : AxisIteratorBase, LookaheadIterator
        {

            private readonly JArrayList attList = new JArrayList(10); //ArrayList<XmlNode> attList = new ArrayList<>(10);
            private int ix = 0;
            private readonly DotNetNodeWrapper start;
            private DotNetNodeWrapper current;
            private DotNetDocumentWrapper docWrapper;
            private bool disposedValue;

            public AttributeEnumeration(DotNetDocumentWrapper docWrapper, DotNetNodeWrapper start)
            {
                this.docWrapper = docWrapper;
                this.start = start;
                XmlNamedNodeMap atts = start.node.Attributes;
                if (atts != null)
                {
                    for (int i = 0; i < atts.Count; i++)
                    {
                        string name = atts.Item(i).Name;
                        if (!(name.StartsWith("xmlns") &&
                                (name.Length == 5 || name[5] == ':')))
                        {
                            attList.add(atts.Item(i));
                        }
                    }
                }
                ix = 0;
            }

            //@Override
            public bool supportsHasNext()
            {
                return true;
            }

            //@Override
            public bool hasNext()
            {
                return ix < attList.size();
            }

            //@Override
            public override NodeInfo next()
            {
                if (ix >= attList.size())
                {
                    return null;
                }
                current = makeWrapper((XmlNode)attList.get(ix), docWrapper, start, ix);
                ix++;
                return current;
            }

            //public Item <bridge>next()
            //{
            //    throw new NotImplementedException();
            //}

            Item SequenceIterator.next()
            {
                return ((AxisIterator)this).next();
            }

            //public void close()
            //{
            //    SequenceIterator.__DefaultMethods.close(this);
            //}

            //public void discharge()
            //{
            //    SequenceIterator.__DefaultMethods.discharge(this);
            //}

            //private void Dispose(bool disposing)
            //{
            //    if (!disposedValue)
            //    {
            //        if (disposing)
            //        {
            //            // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            //        }

            //        // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            //        // TODO: Große Felder auf NULL setzen
            //        disposedValue = true;
            //    }
            //}

            // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
            // ~AttributeEnumeration()
            // {
            //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            //     Dispose(disposing: false);
            // }

            //public void Dispose()
            //{
            //    // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            //    Dispose(disposing: true);
            //    GC.SuppressFinalize(this);
            //}

            //Item AxisIterator.<bridge>next()
            //{
            //    throw new NotImplementedException();
            //}
        }


        //private class AttributeEnumeration : AxisIterator, LookaheadIterator {

        //private JArrayList attList = new JArrayList(10);
        //private int ix = 0;
        //private  DotNetNodeWrapper start;
        //private DotNetNodeWrapper current;

        //public AttributeEnumeration(DotNetNodeWrapper start)
        //{
        //    this.start = start;
        //    XmlNamedNodeMap atts = start.node.Attributes;
        //    if (atts != null)
        //    {
        //        for (int i = 0; i < atts.Count; i++)
        //        {
        //            string name = atts.Item(i).Name;
        //            if (!(name.StartsWith("xmlns") &&
        //                    (name.Length == 5 || name[5] == ':')))
        //            {
        //                attList.add(atts.Item(i));
        //            }
        //        }
        //    }
        //    ix = 0;
        //}

        //// @Override
        //public bool hasNext()
        //{
        //    return ix < attList.size();
        //}

        //// @Override
        //public NodeInfo next()
        //{
        //    if (ix >= attList.size())
        //    {
        //        return null;
        //    }
        //    current = makeWrapper(attList.get(ix), docWrapper, start, ix);
        //    ix++;
        //    return current;
        //}

        ///**
        // * Get properties of this iterator, as a bit-significant integer.
        // *
        // * @return the properties of this iterator. This will be some combination of
        // *         properties such as {@link net.sf.saxon.om.SequenceIterator.Property#GROUNDED},
        // *         {@link net.sf.saxon.om.SequenceIterator.Property#LAST_POSITION_FINDER},
        // *         and {@link net.sf.saxon.om.SequenceIterator.Property#LOOKAHEAD}. It is always
        // *         acceptable to return the value zero, indicating that there are no known special properties.
        // *         It is acceptable for the properties of the iterator to change depending on its state.
        // */

        //// @Override
        //public JEnumSet getProperties() //EnumSet<Property> getProperties()
        //{
        //    return JEnumSet.of(Property.LOOKAHEAD);
        //}
        //}


        /**
         * The class ChildEnumeration handles not only the child axis, but also the
         * following-sibling and preceding-sibling axes. It can also iterate the children
         * of the start node in reverse order, something that is needed to support the
         * preceding and preceding-or-ancestor axes (the latter being used by xsl:number)
         */

        private class ChildEnumeration : AxisIteratorBase, LookaheadIterator
        {

            private DotNetNodeWrapper container;
            private DotNetNodeWrapper start;
            private JArrayList items = new JArrayList(20);// ArrayList<DotNetNodeWrapper> items = new ArrayList<DotNetNodeWrapper>(20);
            private int ix;
            private int position;
            private bool downwards;  // iterate children of start node (not siblings)
            private bool forwards;   // iterate in document order (not reverse order)
            private bool disposedValue;

            public ChildEnumeration(DotNetNodeWrapper start,
                                    bool downwards, bool forwards)
            {
                this.container = this.start;
                this.start = start;
                this.downwards = downwards;
                this.forwards = forwards;
                position = 0;

                DotNetNodeWrapper commonParent;
                if (downwards)
                {
                    commonParent = start;
                }
                else
                {
                    commonParent = (DotNetNodeWrapper)start.getParent();
                }

                XmlNodeList childNodes = commonParent.node.ChildNodes;
                if (downwards)
                {
                    if (!forwards)
                    {
                        // backwards enumeration: go to the end
                        ix = childNodes.Count - 1;
                    }
                }
                else
                {
                    ix = start.getSiblingPosition() + (forwards ? container.span : -1);
                }

                if (forwards)
                {
                    bool previousText = false;
                    for (int i = ix; i < childNodes.Count; i++)
                    {
                        bool thisText = false;
                        XmlNode node = childNodes.Item(i);
                        switch (node.NodeType)
                        {
                            case XmlNodeType.DocumentType:
                            case XmlNodeType.XmlDeclaration:
                                break;
                            case XmlNodeType.EntityReference:
                            //                            System.err.println("Found an entity reference node:");
                            //                            System.err.println("Name: " + node.get_Name());
                            //                            System.err.println("InnerText: " + node.get_InnerText());
                            //                            System.err.println("Previous: " + node.get_PreviousSibling());
                            //                            System.err.println("Previous.InnerText: " + (node.get_PreviousSibling()==null ? "null" : node.get_PreviousSibling().get_InnerText()));
                            //                            System.err.println("Next: " + node.get_NextSibling());
                            //                            System.err.println("Next.InnerText: " + (node.get_NextSibling()==null ? "null" : node.get_NextSibling().get_InnerText()));
                            //                            break;
                            case XmlNodeType.Text:
                            case XmlNodeType.CDATA:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                thisText = true;
                                if (previousText)
                                {
                                    //                                if (isAtomizing()) {
                                    //                                    UntypedAtomicValue old = (UntypedAtomicValue)(items.get(items.size()-1));
                                    //                                    String newval = old.getStringValue() + getStringValue(node, node.get_NodeType().Value);
                                    //                                    items.set(items.size()-1, new UntypedAtomicValue(newval));
                                    //                                } else {
                                    DotNetNodeWrapper old = (DotNetNodeWrapper)items.get(items.size() - 1);
                                    old.span++;
                                    //                                }
                                    break;
                                }
                                // otherwise fall through to default case
                                goto default;
                            default:
                                previousText = thisText;
                                //                            if (isAtomizing()) {
                                //                                items.add(new UntypedAtomicValue(
                                //                                        getStringValue(node, node.get_NodeType().Value)));
                                //                            } else {
                                items.add(makeWrapper(node, container.docWrapper, commonParent, i));
                                //
                                break;
                        }
                    }
                }
                else
                {
                    bool previousText = false;
                    for (int i = ix; i >= 0; i--)
                    {
                        bool thisText = false;
                        XmlNode node = childNodes.Item(i);
                        switch (node.NodeType)
                        {
                            case XmlNodeType.DocumentType:
                            case XmlNodeType.XmlDeclaration:
                                break;
                            case XmlNodeType.EntityReference:
                            //                            System.err.println("Found an entity reference node:");
                            //                            System.err.println("Name: " + node.get_Name());
                            //                            System.err.println("InnerText: " + node.get_InnerText());
                            //                            System.err.println("Previous: " + node.get_PreviousSibling());
                            //                            System.err.println("Previous.InnerText: " + (node.get_PreviousSibling()==null ? "null" : node.get_PreviousSibling().get_InnerText()));
                            //                            System.err.println("Next: " + node.get_NextSibling());
                            //                            System.err.println("Next.InnerText: " + (node.get_NextSibling()==null ? "null" : node.get_NextSibling().get_InnerText()));
                            //                            break;
                            case XmlNodeType.Text:
                            case XmlNodeType.CDATA:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                thisText = true;
                                if (previousText)
                                {
                                    //                                if (isAtomizing()) {
                                    //                                    UntypedAtomicValue old = (UntypedAtomicValue)(items.get(items.size()-1));
                                    //                                    String newval = old.getStringValue() + getStringValue(node, node.get_NodeType().Value);
                                    //                                    items.set(items.size()-1, new UntypedAtomicValue(newval));
                                    //                                } else {
                                    DotNetNodeWrapper old = (DotNetNodeWrapper)items.get(items.size() - 1);
                                    old.node = node;
                                    old.span++;
                                    //                                }
                                    break;
                                }
                                // otherwise fall through to default case
                                goto default;
                            default:
                                previousText = thisText;
                                //                            if (isAtomizing()) {
                                //                                items.add(new UntypedAtomicValue(
                                //                                        getStringValue(node, node.get_NodeType().Value)));
                                //                            } else {
                                items.add(makeWrapper(node, container.docWrapper, commonParent, i));
                                //
                                break;
                        }
                    }
                }
            }

            // @Override
            public bool hasNext()
            {
                return position < items.size();
            }

            public bool supportsHasNext()
            {
                return true;
            }

            /*@Nullable*/
            // @Override
            public override NodeInfo next()
            {
                if (position > -1 && position < items.size())
                {
                    return (NodeInfo)items.get(position++);
                }
                else
                {
                    position = -1;
                    return null;
                }
            }

            Item SequenceIterator.next()
            {
                return this.next();
            }



            //public void close()
            //{
            //    SequenceIterator.__DefaultMethods.close(this);
            //}

            //public void discharge()
            //{
            //    SequenceIterator.__DefaultMethods.discharge(this);
            //}

            //private void Dispose(bool disposing)
            //{
            //    if (!disposedValue)
            //    {
            //        if (disposing)
            //        {
            //            // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            //        }

            //        // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            //        // TODO: Große Felder auf NULL setzen
            //        disposedValue = true;
            //    }
            //}

            //public void Dispose()
            //{
            //    // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            //    Dispose(disposing: true);
            //    GC.SuppressFinalize(this);
            //}

            ///**
            // * Get properties of this iterator, as a bit-significant integer.
            // *
            // * @return the properties of this iterator. This will be some combination of
            // *         properties such as {@link net.sf.saxon.om.SequenceIterator.Property#GROUNDED},
            // *         {@link net.sf.saxon.om.SequenceIterator.Property#LAST_POSITION_FINDER},
            // *         and {@link net.sf.saxon.om.SequenceIterator.Property#LOOKAHEAD}. It is always
            // *         acceptable to return the value zero, indicating that there are no known special properties.
            // *         It is acceptable for the properties of the iterator to change depending on its state.
            // */

            //// @Override
            //    public JEnumSet getProperties()// EnumSet<Property> getProperties()
            //{
            //    return JEnumSet.of(Property.LOOKAHEAD);
            //}

        } // end of class ChildEnumeration


    }
}
